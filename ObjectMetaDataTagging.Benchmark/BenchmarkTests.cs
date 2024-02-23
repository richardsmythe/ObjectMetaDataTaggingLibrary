﻿using BenchmarkDotNet.Attributes;
using ObjectMetaDataTagging.Helpers;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Services;


namespace ObjectMetaDataTagging.Benchmark
{
    [MemoryDiagnoser]
    [SimpleJob(launchCount:1, warmupCount:1, iterationCount: 1)]

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
        public async Task SetsTagAsyncBenchmark()
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
