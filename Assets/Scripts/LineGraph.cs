using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class DataPoint
{
    public string timeStamp;
    public int recordedValue;
}

public class LineGraph : MonoBehaviour
{
    [Header("Graph Objects")]
    [SerializeField]
    private RectTransform _graphContainer;

    [SerializeField]
    private RectTransform _tempValueX,
        _tempValueY;

    [SerializeField]
    private RectTransform _tempGridlineX,
        _tempGridlineY;

    private float _graphWidth,
        _graphHeight;
    
    [Header("Graph Data")]
    [SerializeField]
    private Sprite _dataPointSprite;
    [SerializeField]
    private Vector2 _dataPointSize = new Vector2(15f, 15f);
    [SerializeField]
    private List<DataPoint> _dataList = new List<DataPoint>();
    private List<DataPoint> _dataToGraphList = new List<DataPoint>();
    private DateTime _selectionDate;

    private DateTime _selectedDate;
    private DateTime[] _timePeriods = new DateTime[]
    {
        new DateTime(2001, 01, 01, 0, 0, 0), // Midnight
        new DateTime(2001, 01, 01, 4, 0, 0), // 4 AM
        new DateTime(2001, 01, 01, 8, 0, 0), // 8 AM
        new DateTime(2001, 01, 01, 12, 0, 0), // 12 PM
        new DateTime(2001, 01, 01, 16, 0, 0), // 4 PM
        new DateTime(2001, 01, 01, 20, 0, 0), // 8 PM
        new DateTime(2001, 01, 01, 0, 0, 0) // Midnight
    };

    [Header("Axes Values")]
    [SerializeField]
    private XAxis _xAxis;
    [SerializeField]
    private YAxis _yAxis;

    private DateTime _dataPointTimeStamp;
    private float _dataPointMinutes;
    private float _totalMinutes;

    private List<GameObject> _graphedObjList = new List<GameObject>();

    private GameObject _lastDataPoint;
    private GameObject _newDataPoint;
    private GameObject _dataPointObj;
    private RectTransform _dataPointRect;
    private Vector2 _dataPosition;

    private GameObject _dataConnector;
    private GameObject _connectorObj;
    private RectTransform _connectorRect;
    private Vector2 _connectorDirection;
    private float _connectorDistance;
    private float _connectorAngle;
    [Space, SerializeField]
    private Color _connectorColor = new Color(0, 0, 0, 0.25f);

    private Vector2 _defaultVector = Vector2.zero;
    private Vector3 _defaultScale = Vector3.one;

    private void OnEnable()
    {
        _graphWidth = _graphContainer.sizeDelta.x;
        _graphHeight = _graphContainer.sizeDelta.y;

        SetXAxisMinMax();

        _selectedDate = new DateTime(2021, 03, 27);// DateTime.Today;

        CompileGraphData(_selectedDate);
    }

    private void CompileGraphData(DateTime selectedDate)
    {
        foreach (var dataPoint in _dataList)
        {
            _selectionDate = DateTime.Parse(dataPoint.timeStamp);

            if (_selectionDate.Date == selectedDate)
            {
                _dataToGraphList.Add(dataPoint);
            }
        }

        OrderDataByAscending(_dataToGraphList);
    }

    private void OrderDataByAscending(List<DataPoint> dataToGraph)
    {
        var dataSeries = dataToGraph.OrderByDescending(data => DateTime.Parse(data.timeStamp)).Reverse().ToList();

        GraphDataSeries(dataSeries);
    }

    private void GraphDataSeries(List<DataPoint> dataSeries)
    {
        _graphedObjList.ForEach(obj => Destroy(obj));
        _graphedObjList.Clear();
        
        SetYAxisMinMax(dataSeries);

        PlotXAxisLabels();

        PlotYAxisLabels();

        PlotDataPoints(dataSeries);
    }

    private void SetXAxisMinMax()
    {
        _xAxis.minDateTime = _timePeriods[0];
        
        _xAxis.maxDateTime = _timePeriods[5].AddMinutes(240); // take time period before the end of the day and add minutes or else will be viewed as the start of the day 
    }

    private void SetYAxisMinMax(List<DataPoint> dataSeries)
    {
        if (dataSeries.Count > 0)
        {
            _yAxis.minValue = dataSeries[0].recordedValue;
            _yAxis.maxValue = dataSeries[0].recordedValue;

            for (int i = 0; i < dataSeries.Count; i++)
            {
                _yAxis.currentValue = dataSeries[i].recordedValue;

                if (_yAxis.currentValue < _yAxis.minValue)
                {
                    _yAxis.minValue = _yAxis.currentValue;
                }

                if (_yAxis.currentValue > _yAxis.maxValue)
                {
                    _yAxis.maxValue = _yAxis.currentValue;
                }
            }

            _yAxis.valueRange = _yAxis.maxValue - _yAxis.minValue;

            if (_yAxis.valueRange <= 0)
            {
                _yAxis.valueRange = 1f;
            }

            _yAxis.minValue -= (_yAxis.valueRange * _yAxis.edgeBuffer);
            _yAxis.maxValue += (_yAxis.valueRange * _yAxis.edgeBuffer);
        }
        else
        {
            _yAxis.minValue = _yAxis.defaultMinValue;
            _yAxis.maxValue = _yAxis.defaultMaxValue;
        }
    }

    private void PlotXAxisLabels()
    {        
        _xAxis.minLabelPos = 0f;
        _xAxis.maxLabelPos = 0f;

        _xAxis.labelCount = _timePeriods.Length;

        _xAxis.labelIndex = 0;

        for (int i = 0; i < _xAxis.labelCount; i++)
        {
            // Labels
            _xAxis.currentLabelSpread = _graphWidth / (_xAxis.labelCount + _xAxis.edgeBuffer);
            _xAxis.currentLabelPos = _xAxis.currentLabelSpread + _xAxis.labelIndex * _xAxis.currentLabelSpread;

            if (i == 0)
            {
                _xAxis.minLabelPos = _xAxis.currentLabelPos;
            }
            else if (i == _xAxis.labelCount - 1)
            {
                _xAxis.maxLabelPos = _xAxis.currentLabelPos;
            }

            _xAxis.labelRect = Instantiate(_tempValueX);
            _xAxis.labelRect.SetParent(_graphContainer);
            _xAxis.labelRect.gameObject.SetActive(true);
            _xAxis.labelRect.anchoredPosition = new Vector2(_xAxis.currentLabelPos, _xAxis.labelRect.position.y);
            _xAxis.labelRect.GetComponent<TextMeshProUGUI>().text = _timePeriods[i].ToString("h tt");
            _xAxis.labelRect.localScale = _defaultScale;

            _graphedObjList.Add(_xAxis.labelRect.gameObject);

            // Gridlines
            _xAxis.gridlineRect = Instantiate(_tempGridlineX);
            _xAxis.gridlineRect.SetParent(_graphContainer);
            _xAxis.gridlineRect.gameObject.SetActive(true);
            _xAxis.gridlineRect.anchoredPosition = new Vector2(_xAxis.currentLabelPos, _xAxis.gridlineRect.position.y);
            _xAxis.gridlineRect.localScale = _defaultScale;

            _graphedObjList.Add(_xAxis.gridlineRect.gameObject);

            _xAxis.labelIndex++;
        }
    }

    private void PlotYAxisLabels()
    {
        _yAxis.tempLabelCount = _yAxis.labelCount; // Label count set in Inspector based on preference

        if (_yAxis.tempLabelCount > _yAxis.valueRange)
        {
            int addTo(int to)
            {
                return (to % 2 == 0) ? to : (to + 2);
            }

            if (_yAxis.valueRange % 2 != 0)
            {
                _yAxis.tempLabelCount = addTo((int)_yAxis.valueRange);
            }
            else
            {
                _yAxis.tempLabelCount = (int)_yAxis.valueRange;
            }

            if (_yAxis.valueRange == 1)
            {
                _yAxis.tempLabelCount = Mathf.RoundToInt(_yAxis.valueRange) + 3;
                _yAxis.minValue -= 2;
                _yAxis.maxValue += 2;
            }
        }

        for (int i = 0; i <= _yAxis.tempLabelCount; i++)
        {
            _yAxis.labelPosNormal = (i * 1f) / _yAxis.tempLabelCount;

            _yAxis.labelPos = _yAxis.minValue + (_yAxis.labelPosNormal * (_yAxis.maxValue - _yAxis.minValue));

            // Labels
            _yAxis.labelRect = Instantiate(_tempValueY);
            _yAxis.labelRect.SetParent(_graphContainer);
            _yAxis.labelRect.gameObject.SetActive(true);
            _yAxis.labelRect.anchoredPosition = new Vector2(_yAxis.labelRect.position.x, _yAxis.labelPosNormal * _graphHeight);
            _yAxis.labelRect.GetComponent<TextMeshProUGUI>().text = Mathf.RoundToInt(_yAxis.labelPos).ToString();
            _yAxis.labelRect.localScale = _defaultScale;

            _graphedObjList.Add(_yAxis.labelRect.gameObject);

            // Gridlines
            if (i != 0 && i != _yAxis.tempLabelCount)
            {
                _yAxis.gridlineRect = Instantiate(_tempGridlineY);
                _yAxis.gridlineRect.SetParent(_graphContainer);
                _yAxis.gridlineRect.gameObject.SetActive(true);
                _yAxis.gridlineRect.anchoredPosition = new Vector2(_yAxis.gridlineRect.position.x, _yAxis.labelPosNormal * _graphHeight);
                _yAxis.gridlineRect.localScale = _defaultScale;

                _graphedObjList.Add(_yAxis.gridlineRect.gameObject);
            }
        }
    }

    private void PlotDataPoints(List<DataPoint> dataSeries)
    {
        _lastDataPoint = null;

        _xAxis.totalTime = TimeSpan.FromTicks(_xAxis.maxDateTime.Ticks - _xAxis.minDateTime.Ticks);

        for (int i = 0; i < dataSeries.Count; i++)
        {
            _dataPointTimeStamp = DateTime.Parse(dataSeries[i].timeStamp);

            _dataPointMinutes = (float)(_dataPointTimeStamp.TimeOfDay.TotalMinutes - _xAxis.minDateTime.TimeOfDay.TotalMinutes); 

            _totalMinutes = (float)_xAxis.totalTime.TotalMinutes;

            _xAxis.minMaxLabelVariance = _xAxis.maxLabelPos - _xAxis.minLabelPos;

            _xAxis.graphPos = (_dataPointMinutes / _totalMinutes) * _xAxis.minMaxLabelVariance + _xAxis.minLabelPos;
            _yAxis.graphPos = ((dataSeries[i].recordedValue - _yAxis.minValue) / (_yAxis.maxValue - _yAxis.minValue)) * _graphHeight;

            _dataPosition = new Vector2(_xAxis.graphPos, _yAxis.graphPos);

            _newDataPoint = CreateDataPoint(_dataPosition);

            _graphedObjList.Add(_newDataPoint);

            if (_lastDataPoint != null)
            {
                _dataConnector = CreateDataConnector(_lastDataPoint.GetComponent<RectTransform>().anchoredPosition, 
                    _newDataPoint.GetComponent<RectTransform>().anchoredPosition);

                _graphedObjList.Add(_dataConnector);
            }
            
            _lastDataPoint = _newDataPoint;
        }
    }

    private GameObject CreateDataPoint(Vector2 pos)
    {
        _dataPointObj = new GameObject("Data", typeof(Image));
        _dataPointObj.transform.SetParent(_graphContainer, false);
        _dataPointObj.GetComponent<Image>().sprite = _dataPointSprite;
        
        _dataPointRect = _dataPointObj.GetComponent<RectTransform>();
        _dataPointRect.anchoredPosition = pos;
        _dataPointRect.sizeDelta = _dataPointSize;
        _dataPointRect.anchorMax = _defaultVector;
        _dataPointRect.anchorMin = _defaultVector;

        return _dataPointObj;
    }

    private GameObject CreateDataConnector(Vector2 pointA, Vector2 pointB)
    {
        _connectorObj = new GameObject("Connection", typeof(Image));
        _connectorObj.transform.SetParent(_graphContainer, false);
        _connectorObj.GetComponent<Image>().color = _connectorColor;

        _connectorDirection = (pointB - pointA).normalized;

        _connectorDistance = Vector2.Distance(pointA, pointB);

        _connectorAngle = Mathf.Atan2(_connectorDirection.y, _connectorDirection.x) * Mathf.Rad2Deg;

        _connectorRect = _connectorObj.GetComponent<RectTransform>();
        _connectorRect.anchoredPosition = pointA + _connectorDirection * _connectorDistance * 0.5f;
        _connectorRect.sizeDelta = new Vector2(_connectorDistance, _yAxis.gridlineWidth);
        _connectorRect.anchorMin = _defaultVector;
        _connectorRect.anchorMax = _defaultVector;
        _connectorRect.localEulerAngles = new Vector3(0, 0, _connectorAngle);

        return _connectorObj;
    }
}


