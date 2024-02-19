using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Mvc;

namespace CPInterview.Controllers
{
    [ApiController]
    [Route("wordcount")]
    public class WordCountController : ControllerBase
    {
        private const string BucketName = "checkpoint-test-bucket";

        [HttpPost("countinfile")]
        public async Task<IActionResult> CountWordsInFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File was not valid or empty");
            }
            Dictionary<string, int> counts = CountWords(file);

            UploadResultsToS3(counts);
            return Ok($"the following word counts were uploaded to the bucket: {counts}\n");
        }

        private static Dictionary<string, int> CountWords(IFormFile file)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>();

            using (StreamReader reader = new StreamReader(file.OpenReadStream()))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] words = line.Split(new char[] { ' ' });
                    foreach (string word in words)
                    {
                        string standardized = word.Trim().ToLower();
                        if (standardized.Length == 0)
                        {
                            continue;
                        }
                        if (counts.ContainsKey(standardized))
                        {
                            ++counts[standardized];
                        }
                        else
                        {
                            counts[standardized] = 1;
                        }
                    }
                }
            }

            return counts;
        }

        private void UploadResultsToS3(Dictionary<string, int> counts)
        {
            using (AmazonS3Client client = new AmazonS3Client())
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        foreach (var kvp in counts)
                        {
                            writer.WriteLine($"{kvp.Key}: {kvp.Value}");
                        }
                        writer.Flush();
                        stream.Position = 0;

                        var transferUtility = new TransferUtility(client);
                        transferUtility.Upload(stream, BucketName, "WordCounts.txt");
                    }
                }
            }
        }
    }
}