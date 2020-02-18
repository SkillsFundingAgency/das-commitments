using System;
using System.Collections.Generic;
using CsvHelper;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Shared.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.Services.CreateCsvServiceTests
{
    public class WhenICreateACsvFile
    {
        [Test, MoqAutoData]
        public void Then_The_First_Line_Of_The_File_Is_The_Headers(
            List<TestModel> listToWriteToCsv,
            CreateCsvService createCsvService)
        {
            var actual = createCsvService.GenerateCsvContent(listToWriteToCsv);

            Assert.IsNotNull(actual);
            Assert.IsNotEmpty(actual);
            Assert.IsAssignableFrom<byte[]>(actual);
            var fileString = System.Text.Encoding.Default.GetString(actual);
            var headerLine = fileString.Split(Environment.NewLine)[0];

            Assert.That(headerLine.Contains(nameof(TestModel.Id)));
            Assert.That(!headerLine.Contains(nameof(TestModel.TestValue)));
        }

        [Test, MoqAutoData]
        public void Then_The_Csv_File_Content_Is_Generated(
            List<TestModel> listToWriteToCsv,
            CreateCsvService createCsvService)
        {
            var actual = createCsvService.GenerateCsvContent(listToWriteToCsv);

            Assert.IsNotNull(actual);
            Assert.IsNotEmpty(actual);
            Assert.IsAssignableFrom<byte[]>(actual);
            var fileString = System.Text.Encoding.Default.GetString(actual);
            var lines = fileString.Split(Environment.NewLine);
            Assert.AreEqual(listToWriteToCsv.Count + 2, lines.Length);
            Assert.AreEqual(listToWriteToCsv[0].Description, lines[1].Split(',')[1]);
        }

        [Test, MoqAutoData]
        public void And_Nothing_Is_Passed_To_The_Content_Generator_Then_Exception_Is_Thrown(
            CreateCsvService createCsvService)
        {
            List<TestModel> nullList = null;

            Assert.Throws<WriterException>(() => createCsvService.GenerateCsvContent(nullList));
        }
    }

    public class TestModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        [CsvHelper.Configuration.Attributes.Ignore]
        public long TestValue { get; set; }
    }
}