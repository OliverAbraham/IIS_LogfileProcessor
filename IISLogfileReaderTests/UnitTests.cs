using NUnit.Framework;
using FluentAssertions;
using System.Collections.Generic;
using Abraham.Web;

namespace Tests
{
	public class Tests
	{ 
		[Test]
		public void Should_abort_with_non_existing_source_folder()
		{
			var _sut = new IISLogfileReader();
			_sut.LogfilesFolder = @"";
			

			var Entries = _sut.ReadNewEntries();


			_sut.ErrorMessages.Should().NotBeEmpty();
			_sut.ErrorMessages[0].Should().NotBeEmpty();
			Entries.Should().BeEmpty();
		}

		[Test]
		public void Should_abort_without_files()
		{
			var _sut = new IISLogfileReader();
			_sut.LogfilesFolder = @"..\..\";
			

			var Entries = _sut.ReadNewEntries();


			_sut.ErrorMessages.Should().NotBeEmpty();
			_sut.ErrorMessages[0].Should().NotBeEmpty();
			Entries.Should().BeEmpty();
		}

		[Test]
		public void Should_read_file()
		{
			var _sut = new IISLogfileReader();
			_sut.LogfilesFolder = @"..\..\..\Testdata";
			_sut.DeleteStatus_for_unit_tests_only();


			var Entries = _sut.ReadNewEntries();


			_sut.ErrorMessages.Should().BeEmpty();
			Entries.Should().NotBeEmpty();
			Entries.Count.Should().Be(5);
			Entries[4].Datetime   .Should().Be(new System.DateTime(2019, 7, 25, 15, 42, 26));
			Entries[4].DestIP	  .Should().Be("fe80::809f:b4b:5d92:fb51%2");
			Entries[4].Method 	  .Should().Be("PROPFIND");
			Entries[4].UriStem    .Should().Be("/Hausnet$");
			Entries[4].UriQuery   .Should().Be("-");
			Entries[4].Port 	  .Should().Be(80);
			Entries[4].Username   .Should().Be("-");
			Entries[4].SourceIP   .Should().Be("fe80::317f:714e:11d4:e634%2");
			Entries[4].UserAgent  .Should().Be("Microsoft-WebDAV-MiniRedir/10.0.17763");
			Entries[4].Referer    .Should().Be("-");
			Entries[4].Status 	  .Should().Be(111);
			Entries[4].Substatus  .Should().Be(222);
			Entries[4].Win32Status.Should().Be(333);
			Entries[4].TimeTaken  .Should().Be(444);
		}

		[Test]
		public void Should_read_first_file_in_chunks()
		{
			var _sut = new IISLogfileReader();
			_sut.LogfilesFolder = @"..\..\..\Testdata2";
			_sut.DeleteStatus_for_unit_tests_only();


			var Entries = _sut.ReadNewEntries();


			_sut.ErrorMessages.Should().BeEmpty();
			Entries.Should().NotBeEmpty();
			Entries.Count.Should().Be(1);
			Entries[0].DestIP	  .Should().Be("FIRSTLINE");


			_sut = new IISLogfileReader();
			_sut.LogfilesFolder = @"..\..\..\Testdata3";
			_sut.PatchStatus_for_unit_tests_only("Testdata2", "Testdata3");
			

			Entries = _sut.ReadNewEntries();


			_sut.ErrorMessages.Should().BeEmpty();
			Entries.Should().NotBeEmpty();
			Entries.Count.Should().Be(4);
			Entries[0].DestIP	  .Should().Be("SECONDLINE");
			Entries[1].DestIP	  .Should().Be("THIRDLINE");
			Entries[2].DestIP	  .Should().Be("FOURTHLINE");
			Entries[3].DestIP	  .Should().Be("FIFTHLINE");
		}
	}
}