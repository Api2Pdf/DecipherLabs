using System.Text.Json;
using CSharpVitamins;
using MediatR;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Application.Common.Models;

namespace RaythaZero.Application.Projects.Commands;

public class BeginToGeneratePackage
{

    public record Command : LoggableEntityRequest<CommandResponseDto<ShortGuid>>
    {
    }
    
    public class Handler : IRequestHandler<Command, CommandResponseDto<ShortGuid>>
    {
        
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IRaythaDbContext _db;
        public Handler(
            IBackgroundTaskQueue taskQueue,
            IRaythaDbContext db
        )
        {
            _taskQueue = taskQueue;
            _db = db;
        }
        public async Task<CommandResponseDto<ShortGuid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var backgroundJobId = await _taskQueue.EnqueueAsync<BackgroundTask>(request, cancellationToken);
            return new CommandResponseDto<ShortGuid>(backgroundJobId);
        }
    }

    public class BackgroundTask : IExecuteBackgroundTask
    {
        private readonly IRaythaDbContext _db;
        private readonly IFileStorageProviderSettings _fileStorageProviderSettings;
        private readonly IFileStorageProvider _fileStorageProvider;
        
        public BackgroundTask(
            IRaythaDbContext db,
            IFileStorageProvider fileStorageProvider,
            IFileStorageProviderSettings fileStorageProviderSettings
        )
        {
            _db = db;
            _fileStorageProviderSettings = fileStorageProviderSettings;
            _fileStorageProvider = fileStorageProvider;
        }

        public async Task Execute(Guid jobId, JsonElement args, CancellationToken cancellationToken)
        {
            var job = _db.BackgroundTasks.First(p => p.Id == jobId);
            UpdateStatus(job, "Step 1", 10, cancellationToken);
            Thread.Sleep(5000);
            UpdateStatus(job, "Step 2", 50, cancellationToken);
            Thread.Sleep(10000);
            UpdateStatus(job, "Step 3", 100, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        private async void UpdateStatus(Domain.Entities.BackgroundTask job, string status, int percentComplete, CancellationToken cancellationToken, bool appendStatus = true)
        {
            job.StatusInfo = appendStatus ? job.StatusInfo + "<br/>" + status : status;
            job.PercentComplete = percentComplete;
            _db.BackgroundTasks.Update(job);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}