﻿using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Core.Extension;
using Magicodes.ExporterAndImporter.Excel;
using Magicodes.ExporterAndImporter.Tests.Models.Excel;
using Magicodes.ExporterAndImporter.Tests.Models.Export;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Magicodes.ExporterAndImporter.Tests
{
    public class Excel_Tests : TestBase
    {
        public Excel_Tests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        private readonly ITestOutputHelper _testOutputHelper;
        /// <summary>
        ///    见Issue：https://github.com/dotnetcore/Magicodes.IE/issues/73
        /// </summary>
        /// <returns></returns>
        [Fact(DisplayName = "模板导出单列测试")]
        public async Task ExportByTemplate_SingleCol_Test()
        {
            var tplPath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles", "ExportTemplates",
                "SingleColTemplate.xlsx");
            IExportFileByTemplate exporter = new ExcelExporter();
            var filePath = GetTestFilePath($"{nameof(ExportByTemplate_SingleCol_Test)}.xlsx");
            DeleteFile(filePath);

            var result = await exporter.ExportByTemplate(filePath,
                new ExportTestDataWithSingleColTpl()
                {
                    List = GenFu.GenFu.ListOf<ExportTestDataWithSingleCol>()
                }, tplPath);
            result.ShouldNotBeNull();
            File.Exists(filePath).ShouldBeTrue();
        }
        /// <summary>
        ///     见Issue：https://github.com/dotnetcore/Magicodes.IE/issues/90
        /// </summary>
        /// <returns></returns>
        [Fact(DisplayName = "模板导出多Sheet测试")]
        public async Task ExportByTemplate_Multi_Sheet_Test()
        {
            var tplPath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles", "ExportTemplates",
             "MultiSheet.xlsx");
            IExportFileByTemplate exporter = new ExcelExporter();
            var filePath = GetTestFilePath($"{nameof(ExportByTemplate_Multi_Sheet_Test)}.xlsx");
            DeleteFile(filePath);

            var result = await exporter.ExportByTemplate(filePath,
                new ExportTestDataWithSingleColTpl()
                {
                    List = GenFu.GenFu.ListOf<ExportTestDataWithSingleCol>()
                }, tplPath);
            result.ShouldNotBeNull();
            File.Exists(filePath).ShouldBeTrue();
        }

        /// <summary>
        ///     见Issue：https://github.com/dotnetcore/Magicodes.IE/issues/131
        /// </summary>
        /// <returns></returns>
        [Fact(DisplayName = "模板导出")]
        public async Task ExportByTemplate_Images_Test()
        {
            var tplPath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles", "ExportTemplates",
                "Issue#131.xlsx");
            IExportFileByTemplate exporter = new ExcelExporter();
            var filePath = GetTestFilePath($"{nameof(ExportByTemplate_Images_Test)}.xlsx");
            DeleteFile(filePath);

            var result = await exporter.ExportByTemplate(filePath,
                new Issue131()
                {
                    List = new List<DTO_Product>()
                    {
                        new DTO_Product()
                        {
                            ImageUrl = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles", "issue131.png")
                        }
                    }

                }, tplPath);
            result.ShouldNotBeNull();
            File.Exists(filePath).ShouldBeTrue();
            using (var pck = new ExcelPackage(new FileInfo(filePath)))
            {
                pck.Workbook.Worksheets.Count.ShouldBe(1);
                var ec = pck.Workbook.Worksheets.First();
                var pic = ec.Drawings[0] as ExcelPicture;
                pic.GetPrivateProperty<int>("_height").ShouldBe(120);
                pic.GetPrivateProperty<int>("_width").ShouldBe(120);

            }
        }

        /// <summary>
        /// 见Issue：https://github.com/dotnetcore/Magicodes.IE/issues/53
        /// </summary>
        /// <returns></returns>
        [Fact(DisplayName = "使用同一个Dto导出并导入")]
        public async Task ExportAndImportUseOneDto_Test()
        {
            IExporter exporter = new ExcelExporter();

            var filePath = GetTestFilePath($"{nameof(ExportAndImportUseOneDto_Test)}.xlsx");

            DeleteFile(filePath);

            var data = GenFu.GenFu.ListOf<SalaryInfo>(100);
            data[1].TestDateTimeOffset2 = DateTimeOffset.Now.Date.AddSeconds(123413);
            data[2].TestDateTimeOffset2 = null;
            var result = await exporter.Export(filePath, data);
            result.ShouldNotBeNull();
            File.Exists(filePath).ShouldBeTrue();

            IImporter Importer = new ExcelImporter();
            var importResult = await Importer.Import<SalaryInfo>(filePath);
            if (importResult.HasError)
            {
                _testOutputHelper.WriteLine(importResult.Exception?.ToString());

            }
            importResult.HasError.ShouldBeFalse();

            importResult.Data.Count.ShouldBe(data.Count);
            for (int i = 0; i < importResult.Data.Count; i++)
            {
                var item = importResult.Data.ElementAt(i);
                item.SalaryDate.Date.ShouldBe(data[i].SalaryDate.Date);
                item.PostSalary = data[i].PostSalary;
                item.EmpName = data[i].EmpName;

                item.TestNullDate1.ShouldBe(data[i].TestNullDate1);
                if (item.TestNullDate2.HasValue)
                {
                    item.TestNullDate2.Value.Date.ShouldBe(data[i].TestNullDate2.Value.Date);
                }

                item.TestDateTimeOffset1.ShouldBe(data[i].TestDateTimeOffset1);
                item.TestDateTimeOffset2.ShouldBe(data[i].TestDateTimeOffset2);
            }

        }




    }
}
