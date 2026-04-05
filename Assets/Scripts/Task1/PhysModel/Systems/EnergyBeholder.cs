using Leopotam.Ecs;

using UnityEngine;

using XCharts.Runtime;

namespace Task1 {
    public class EnergyBeholderSystem : IEcsInitSystem, IEcsRunSystem {
        EcsFilter<MaterialPoint> points = null;
        EcsFilter<Spring> springs = null;

        Serie energySerie;
        float systemInitEnergy;

        public EnergyBeholderSystem(LineChart energyChart) {
            energyChart.RemoveData();
            energyChart.name = "energy rate";

            var xAxis = energyChart.EnsureChartComponent<XAxis>();
            xAxis.type = Axis.AxisType.Time;

            var yAxis = energyChart.EnsureChartComponent<YAxis>();
            yAxis.minMaxType = Axis.AxisMinMaxType.MinMaxAuto;

            energySerie = energyChart.AddSerie<Line>("energy");
        }

        float CalculateEnergy() {
            float result = 0;

            foreach (var pointIndex in points) {
                ref var point = ref points.Get1(pointIndex);

                result += 0.5f * point.m * point.v.sqrMagnitude;
            }

            foreach (var springIndex in springs) {
                ref var spring = ref springs.Get1(springIndex);

                var joint1 = spring.joint1.Get<Position>().r;
                var joint2 = spring.joint2.Get<Position>().r;
                var dl = (joint1 - joint2).magnitude - spring.relaxedLength;

                result += 0.5f * spring.k * dl * dl;
            }

            return result;
        }

        public void Init() {
            systemInitEnergy = CalculateEnergy();
        }

        public void Run() {
            var systemEnergy = CalculateEnergy();
            double time = Time.time;
            energySerie.AddData(time, systemEnergy / systemInitEnergy);
        }
    }
}
