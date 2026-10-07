using Quartz;
using WebGenQRCode.Interfaces;

namespace WebGenQRCode.Jobs;

public class DbSeedJob(IDbSeeder dbSeeder) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await dbSeeder.SeedData();
    }
}