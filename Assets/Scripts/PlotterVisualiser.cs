// using UnityEngine;

// using XCharts.Runtime;

// [RequireComponent(typeof(LineChart))]
// class PlotterVisualiser : MonoBehaviour {
//     LineChart chart;
//     Serie serie;

//     private void Start() {
//         chart = GetComponent<LineChart>();
//         chart.RemoveData();

//         serie = chart.AddSerie<Line>("plotter");

//         var xAxis = chart.EnsureChartComponent<XAxis>();
//         xAxis.type = Axis.AxisType.Time;
//         xAxis.minMaxType = Axis.AxisMinMaxType.Custom;
//         // Устанавливаем начальный диапазон времени
//         xAxis.min = 0;
//         xAxis.max = 5; // Показывать последние 5 секунд

//         var yAxis = chart.EnsureChartComponent<YAxis>();
//         yAxis.minMaxType = Axis.AxisMinMaxType.MinMaxAuto;
//     }

//     private void FixedUpdate() {
//         double time = Time.time;
//         double y = Mathf.Sin((float)time);

//         // Добавляем точку с координатами (время, значение)
//         serie.AddData(time, y);

//         // Обновляем диапазон оси X, чтобы показывать последние 5 секунд
//         var xAxis = chart.EnsureChartComponent<XAxis>();
//         xAxis.min = time - 5;
//         xAxis.max = time;
//     }
// }
