﻿using Dapper;
using Microsoft.Extensions.Configuration;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Domain.Entities;
using System.Data;

namespace RaythaZero.Infrastructure.BackgroundTasks;

public class BackgroundTaskDb : IBackgroundTaskDb
{
    private readonly IDbConnection _db;
    private readonly IConfiguration _configuration;
    public BackgroundTaskDb(IDbConnection db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public BackgroundTask DequeueBackgroundTask()
    {
        BackgroundTask bgTask;
        if (_db.State == ConnectionState.Closed)
        {
            _db.Open();
        }
        using (var transaction = _db.BeginTransaction())
        {
            bgTask = _db.QueryFirstOrDefault<BackgroundTask>("SELECT * FROM \"BackgroundTasks\" WHERE \"Status\" = @status ORDER BY \"CreationTime\" ASC FOR UPDATE SKIP LOCKED", new { status = BackgroundTaskStatus.Enqueued.DeveloperName }, transaction: transaction); 
            if (bgTask != null)
            {
                _db.Execute("UPDATE \"BackgroundTasks\" SET \"Status\" = @status WHERE \"Id\" = @id", new { status = BackgroundTaskStatus.Processing.DeveloperName, id = bgTask.Id }, transaction: transaction);
            }
            transaction.Commit();
        }
        return bgTask;
    }
}
