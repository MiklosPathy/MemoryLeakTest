using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Timers;

namespace EFMemoryLeakTest
{
    public sealed class TestHostedService : IHostedService
    {
        const bool AutoReset = true;
        const int Interval = 10;
        const bool Using = false;
        const bool AsNoTracking = false;
        private readonly double StressPeriod = TimeSpan.FromSeconds(30).TotalMicroseconds;
        private readonly double ChillPeriod = TimeSpan.FromSeconds(5).TotalMicroseconds;

        private readonly IDbContextFactory<TestBDContext> dbContextFactory;
        private readonly ILogger<TestHostedService> logger;
        private System.Timers.Timer timer1;
        private System.Timers.Timer timer2;
        private System.Timers.Timer chilltimer;
        private int iterations = 1;

        public TestHostedService(IDbContextFactory<TestBDContext> dbContextFactory, ILogger<TestHostedService> logger)
        {
            this.dbContextFactory = dbContextFactory;
            this.logger = logger;
            timer1 = new System.Timers.Timer();
            timer1.Interval = Interval;
            timer1.AutoReset = AutoReset;
            timer1.Elapsed += Timer_Elapsed;

            timer2 = new System.Timers.Timer();
            timer2.Interval = Interval;
            timer2.AutoReset = AutoReset;
            timer2.Elapsed += Timer_Elapsed;

            chilltimer = new System.Timers.Timer();
            chilltimer.Interval = StressPeriod;
            chilltimer.AutoReset = false;
            chilltimer.Elapsed += ChillTimer_Elapsed;
        }

        private bool Chill = false;

        private void ChillTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Chill = !Chill;
            if (Chill)
            {
                timer1.Stop();
                timer2.Stop();
                logger.LogInformation("Chill");
                chilltimer.Interval = StressPeriod;
            }
            else
            {
                timer1.Start();
                timer2.Start();
                logger.LogInformation("Stress");
                chilltimer.Interval = ChillPeriod;
            }
            chilltimer.Start();
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            int myiteration = iterations;
            iterations++;

            logger.LogInformation(myiteration.ToString() + " Start");

            void QueryTest(TestBDContext dbContext)
            {
                var query = dbContext.TestEntities.AsQueryable();
                if (AsNoTracking) query = query.AsNoTracking();
                var data = query.ToList();
                var first = data.First();
            }

            if (Using)
            {
                using (var dbContext = dbContextFactory.CreateDbContext())
                {
                    QueryTest(dbContext);
                }
            }
            else
            {
                var dbContext = dbContextFactory.CreateDbContext();
                QueryTest(dbContext);
            }
            logger.LogInformation(myiteration.ToString() + " End");

            if (!AutoReset)
            {
                System.Timers.Timer timer = (System.Timers.Timer)sender;
                timer.Start();
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            timer1.Start();
            timer2.Start();
            chilltimer.Start();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            chilltimer.Stop();
            timer1.Stop();
            timer2.Stop();
            timer1.Elapsed -= Timer_Elapsed;
            timer2.Elapsed -= Timer_Elapsed;
        }
    }
}
