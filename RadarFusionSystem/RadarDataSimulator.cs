using GMap.NET;
using System;
using System.Threading;

namespace RadarMapping
{
    public class RadarDataSimulator
    {
        private static readonly Random RandomGenerator = new Random();

        public void StartSimulation(Action<PointLatLng> updateCallback, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Simulate random radar positions near Dubai
                    var latitude = 25.276987 + RandomGenerator.NextDouble() * 0.02 - 0.01;
                    var longitude = 55.296249 + RandomGenerator.NextDouble() * 0.02 - 0.01;

                    var simulatedPosition = new PointLatLng(latitude, longitude);
                    updateCallback(simulatedPosition);

                    Thread.Sleep(1000); // Simulate data every second
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}
