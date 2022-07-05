using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CsvHelper;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Shared.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.Services.CreateCsvServiceTests
{
    public class WhenICreateACsvFile
    {
        [Test, MoqAutoData]
        public void Then_The_First_Line_Is_A_Comment_Showing_The_Download_Message(
            List<SomethingToCsv> listToWriteToCsv,
            CreateCsvService createCsvService)
        {
            var actual = createCsvService.GenerateCsvContent(listToWriteToCsv, true);

            Assert.IsNotNull(actual);
            Assert.IsAssignableFrom<MemoryStream>(actual);
            var actualByteArray = actual.ToArray();
            var fileString = System.Text.Encoding.Default.GetString(actualByteArray);
            var commentLine = fileString.Split(Environment.NewLine)[0];

            Assert.AreEqual("#Data only includes apprentices with an apprenticeship end date within the last 12 months", commentLine);
        }

        [Test, MoqAutoData]
        public void Then_The_Second_Line_Of_The_File_Is_The_Headers(
            List<SomethingToCsv> listToWriteToCsv,
            CreateCsvService createCsvService)
        {
            var actual = createCsvService.GenerateCsvContent(listToWriteToCsv, true);

            Assert.IsNotNull(actual);
            Assert.IsAssignableFrom<MemoryStream>(actual);
            var actualByteArray = actual.ToArray();
            var fileString = System.Text.Encoding.Default.GetString(actualByteArray);
            var headerLine = fileString.Split(Environment.NewLine)[1];

            Assert.That(headerLine.Contains(nameof(SomethingToCsv.Id)));
            Assert.That(!headerLine.Contains(nameof(SomethingToCsv.InternalStuff)));
        }

        [Test, MoqAutoData]
        public void Then_The_Csv_File_Content_Is_Generated(
            List<SomethingToCsv> listToWriteToCsv,
            CreateCsvService createCsvService)
        {
            var actual = createCsvService.GenerateCsvContent(listToWriteToCsv, true);

            Assert.IsNotNull(actual);
            Assert.IsAssignableFrom<MemoryStream>(actual);
            var actualByteArray = actual.ToArray();
            var fileString = System.Text.Encoding.Default.GetString(actualByteArray);
            var lines = fileString.Split(Environment.NewLine);
            Assert.AreEqual(listToWriteToCsv.Count + 3, lines.Length);
            Assert.AreEqual(listToWriteToCsv[0].Description, lines[2].Split(',')[1]);
        }

        [Test, MoqAutoData]
        public void And_Nothing_Is_Passed_To_The_Content_Generator_Then_Exception_Is_Thrown(
            CreateCsvService createCsvService)
        {
            List<SomethingToCsv> nullList = null;

            Assert.Throws<WriterException>(() => createCsvService.GenerateCsvContent(nullList, false));
        }

        [Test, MoqAutoData]
        public void Then_The_Objects_Are_Disposed_Calling_Dispose(
            List<SomethingToCsv> listToWriteToCsv,
            CreateCsvService createCsvService)
        {
            //Arrange
            var memoryStreamField = typeof(CreateCsvService).GetField("_memoryStream", BindingFlags.NonPublic | BindingFlags.Instance);
            var csvStreamField = typeof(CreateCsvService).GetField("_csvWriter", BindingFlags.NonPublic | BindingFlags.Instance);
            var streamWriterField = typeof(CreateCsvService).GetField("_streamWriter", BindingFlags.NonPublic | BindingFlags.Instance);
            createCsvService.GenerateCsvContent(listToWriteToCsv, true);
            var getterMemoryStream = (MemoryStream)memoryStreamField.GetValue(createCsvService);
            var getterStream = (StreamWriter)streamWriterField.GetValue(createCsvService);
            Assert.IsTrue(getterMemoryStream.CanWrite);
            Assert.IsTrue(getterStream.BaseStream.CanWrite);
            //Assert.IsTrue(((StreamWriter)getterCsvWriter.Context.Writer).BaseStream.CanWrite);

            //Act
            createCsvService.Dispose();

            //Assert
            getterMemoryStream = (MemoryStream)memoryStreamField.GetValue(createCsvService);
            getterStream = (StreamWriter)streamWriterField.GetValue(createCsvService);
            Assert.IsFalse(getterMemoryStream.CanWrite);
            Assert.IsFalse(getterStream.BaseStream.CanWrite);
        }
    }

    public class SomethingToCsv
    {
        public int Id { get; set; }
        public string Description { get; set; }
        [CsvHelper.Configuration.Attributes.Ignore]
        public long InternalStuff { get; set; }
    }
}