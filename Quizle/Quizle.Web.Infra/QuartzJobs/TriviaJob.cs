﻿using Microsoft.Extensions.Configuration;
using Quartz;
using Quizle.Core.Questions.Contracts;
using System.Diagnostics;

namespace Quizle.Web.Infra.QuartzJobs
{
    public class TriviaJob : IJob
    {
        private readonly ITriviaDataService _triviaDataService;
        private readonly IConfiguration _config;
        public TriviaJob(ITriviaDataService triviaDataService, IConfiguration configuration)
        {
            _triviaDataService = triviaDataService;
            _config = configuration;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var triviaData = await _triviaDataService.GetDataAsync(_config.GetValue<string>("TriviaUrl"));
        }
    }
}
