using Leopotam.Ecs;

using UnityEngine;

using XCharts.Runtime;

namespace Task3 {
    /// <summary>
    /// Система отображения графика энергии и момента импульса с помощью XCharts.
    /// График появляется в левом верхнем углу экрана.
    /// </summary>
    public class EnergyChartSystem : IEcsInitSystem, IEcsRunSystem {
        private EcsFilter<RigidBody, AngularMomentumDisplay> rigidBodies = null;

        private LineChart chart;
        private float displayTimer = 0f;
        private const float DISPLAY_INTERVAL = 0.05f; // Обновляем график каждые 0.05 секунд
        private const int MAX_DATA_POINTS = 200; // Максимальное количество точек на графике

        private float timeElapsed = 0f;

        public void Init() {
            if (rigidBodies.IsEmpty()) return;

            CreateChart();
        }

        private void CreateChart() {
            var canvasObj = new GameObject("EnergyChartCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var canvasScaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var chartObj = new GameObject("EnergyChart");
            chartObj.transform.SetParent(canvasObj.transform, false);

            var rectTransform = chartObj.AddComponent<RectTransform>();

            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = new Vector2(10, -10);
            rectTransform.sizeDelta = new Vector2(600, 400);

            chart = chartObj.AddComponent<LineChart>();

            var title = chart.EnsureChartComponent<Title>();
            title.show = true;
            title.text = "Энергия: стартовая и текущая";
            title.subText = "Симуляция твердого тела";
            title.location = new Location { align = (Location.Align)Align.Left, top = 5, left = 10 };

            var legend = chart.EnsureChartComponent<Legend>();
            legend.show = true;
            legend.location = new Location { align = (Location.Align)Align.Right, top = 35, right = 10 };

            var xAxis = chart.EnsureChartComponent<XAxis>();
            xAxis.type = Axis.AxisType.Value;
            xAxis.boundaryGap = false;
            xAxis.splitNumber = 5;
            xAxis.axisName.name = "Время (с)";

            var yAxis = chart.EnsureChartComponent<YAxis>();
            yAxis.type = Axis.AxisType.Value;
            yAxis.axisName.name = "Отклонение (%)";
            yAxis.splitNumber = 5;

            var tooltip = chart.EnsureChartComponent<Tooltip>();
            tooltip.show = true;
            tooltip.type = Tooltip.Type.Line;

            var serie1 = chart.AddSerie<Line>("Стартовая энергия");
            serie1.lineStyle.width = 2;
            serie1.lineStyle.color = new Color(0.3f, 0.8f, 1f); // Голубой
            serie1.symbol.show = false;
            serie1.animation.enable = false;

            var serie2 = chart.AddSerie<Line>("Текущая энергия");
            serie2.lineStyle.width = 2;
            serie2.lineStyle.color = new Color(1f, 0.3f, 0.3f); // Красный
            serie2.symbol.show = false;
            serie2.animation.enable = false;
        }

        public void Run() {
            if (chart == null) return;

            displayTimer += Time.fixedDeltaTime;
            timeElapsed += Time.fixedDeltaTime;

            if (displayTimer >= DISPLAY_INTERVAL) {
                displayTimer = 0f;

                foreach (var idx in rigidBodies) {
                    ref var display = ref rigidBodies.Get2(idx);

                    chart.AddXAxisData(timeElapsed.ToString("F2"));
                    chart.AddData(0, display.initialEnergy);
                    chart.AddData(1, display.currentEnergy);
                }
            }
        }
    }
}
