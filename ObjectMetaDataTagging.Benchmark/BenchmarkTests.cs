using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using ObjectMetaDataTagging.Helpers;
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
        private readonly TagFactory _tagFactory;

        public BenchmarkTests()
        {
            _tagFactory = new TagFactory();
            _inMemoryTaggingService = new InMemoryTaggingService<BaseTag>();
        }

        [Benchmark]
        public async Task SetSTagAsyncBenchmark()
        {   
            var testObject = new PersonTranscation();
            var tag = new BaseTag("TagName", "TagValue", "TagDescription");

            await _inMemoryTaggingService.SetTagAsync(testObject, tag);
        }

        [Benchmark]
        public async Task SetMultipleTagsAsyncBenchmark()
        {
            var testObject = new PersonTranscation();
            

            var tagData = new List<(string name, object value, string description)>();

            for (int i = 1; i <= 25; i++)
            {
                string tagName = $"Tag{i}";
                string tagValue = $"Value{i}";
                string tagDescription = $"Description{i}";

                tagData.Add((tagName, tagValue, tagDescription));
            }

            IEnumerable<BaseTag> tags = _tagFactory.CreateBaseTags(tagData);

            await _inMemoryTaggingService.BulkAddTagsAsync(testObject, tags);
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
