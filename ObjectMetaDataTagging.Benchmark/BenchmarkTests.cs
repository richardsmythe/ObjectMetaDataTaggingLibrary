using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Services;
using ObjectMetaDataTagging.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ObjectMetaDataTagging.Benchmark
{
    [MemoryDiagnoser]
    [SimpleJob(launchCount: 2, warmupCount: 2, iterationCount: 2)]

    public class BenchmarkTests
    {
        private readonly InMemoryTaggingService<BaseTag> _inMemoryTaggingService;

        public BenchmarkTests()
        {
            _inMemoryTaggingService = new InMemoryTaggingService<BaseTag>();
        }

        [Benchmark]
        public async Task SetTagAsyncBenchmark()
        {   
            var testObject = new PersonTranscation();
            var tag = new BaseTag("TagName", "TagValue", "TagDescription");

            await _inMemoryTaggingService.SetTagAsync(testObject, tag);
        }
    }


    public class PersonTranscation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public double Amount { get; set; }
    }
}
