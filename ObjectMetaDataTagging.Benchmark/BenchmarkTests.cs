using BenchmarkDotNet.Attributes;
using ObjectMetaDataTagging.Helpers;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Services;


namespace ObjectMetaDataTagging.Benchmark
{
    [MemoryDiagnoser]
    [SimpleJob(launchCount:2, warmupCount:2, iterationCount: 2)]

    public class BenchmarkTests
    {
        private readonly InMemoryTaggingService<BaseTag> _inMemoryTaggingService;
        private readonly TagFactory _tagFactory;
        private PersonTranscation _testObject;
        private Guid _lastTagId;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _testObject = new PersonTranscation();
            var tags = new List<BaseTag>();

            for (int i = 1; i <= 100; i++)
            {
                var tag = new BaseTag($"Tag{i}", $"Value{i}", $"Description{i}");
                tags.Add(tag);
            }

            _inMemoryTaggingService.BulkAddTagsAsync(_testObject, tags).GetAwaiter().GetResult();
            _lastTagId = tags.Last().Id;
        }

        public BenchmarkTests()
        {
            _tagFactory = new TagFactory();
            _inMemoryTaggingService = new InMemoryTaggingService<BaseTag>();
        }

        //[Benchmark]
        //public async Task SetsTagAsyncBenchmark()
        //{   
        //    var testObject = new PersonTranscation();
        //    var tag = new BaseTag("TagName", "TagValue", "TagDescription");

        //    await _inMemoryTaggingService.SetTagAsync(testObject, tag);
        //}

        //[Benchmark]
        //public async Task SetMultipleTagsAsyncBenchmark()
        //{
        //    var testObject = new PersonTranscation();            

        //    var tagData = new List<(string name, object value, string description)>();

        //    for (int i = 1; i <= 25; i++)
        //    {
        //        string tagName = $"Tag{i}";
        //        string tagValue = $"Value{i}";
        //        string tagDescription = $"Description{i}";

        //        tagData.Add((tagName, tagValue, tagDescription));
        //    }

        //    IEnumerable<BaseTag> tags = _tagFactory.CreateBaseTags(tagData);

        //    await _inMemoryTaggingService.BulkAddTagsAsync(testObject, tags);
        //}

        //[Benchmark]
        //public async Task SetMultipleObjectsAndTagsAsyncBenchmark()
        //{
        //    for (int i = 0; i < 50; i++)
        //    {
        //        var testObject = new PersonTranscation();

        //        var tagData = new List<(string name, object value, string description)>();

        //        for (int j = 1; j <= 5; j++)
        //        {
        //            string tagName = $"Tag{i}-{j}";
        //            string tagValue = $"Value{i}-{j}";
        //            string tagDescription = $"Description{i}-{j}";

        //            tagData.Add((tagName, tagValue, tagDescription));
        //        }

        //        IEnumerable<BaseTag> tags = _tagFactory.CreateBaseTags(tagData);

        //        await _inMemoryTaggingService.BulkAddTagsAsync(testObject, tags);
        //    }
        //}
        [Benchmark]

        public async Task GetTagBenchmark()
        {
            await _inMemoryTaggingService.GetTag(_testObject, _lastTagId);
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
