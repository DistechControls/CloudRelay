using Distech.CloudRelay.Common.DAL;
using Distech.CloudRelay.Common.Exceptions;
using Distech.CloudRelay.Common.Options;
using Distech.CloudRelay.Common.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Distech.CloudRelay.Common.UnitTests.Services
{
    public class FileServiceTests
    {
        private readonly Mock<ILogger<FileService>> m_LoggerMock;

        public FileServiceTests()
        {
            m_LoggerMock = new Mock<ILogger<FileService>>();
        }

        #region OpenFileAsync

        [Theory()]
        [InlineData(typeof(IdNotFoundException), new object[] { ErrorCodes.BlobNotFound, "" })]
        public async Task OpenFileAsync_ApiException_KnownApiExceptionAreSwalllowed(Type apiExceptionType, object[] exCtorParam)
        {
            //Arrange
            var blobRepoMock = CreateDefaultBlobRepository(new List<BlobInfo>());
            blobRepoMock.Setup(m => m.OpenBlobAsync(It.IsAny<string>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(apiExceptionType, exCtorParam));
            var optionsMock = new Mock<IOptionsSnapshot<FileStorageOptions>>();
            optionsMock.Setup(m => m.Value).Returns(new FileStorageOptions());

            //Act
            var service = new FileService(blobRepoMock.Object, optionsMock.Object, m_LoggerMock.Object);
            Func<Task> act = () => service.OpenFileAsync("foo", "bar");

            //Assert
            var ex = await Assert.ThrowsAnyAsync<Exception>(act);
            Assert.False(typeof(ApiException).IsAssignableFrom(ex.GetType()));
        }

        #endregion

        #region WriteFileAsync

        [Theory()]
        [InlineData(typeof(ConflictException), new object[] { ErrorCodes.BlobAlreadyExists })]
        [InlineData(typeof(IdNotFoundException), new object[] { ErrorCodes.BlobNotFound, "" })]
        public async Task WriteFileAsync_ApiException_KnownApiExceptionAreSwalllowed(Type apiExceptionType, object[] exCtorParam)
        {
            //Arrange
            var blobRepoMock = CreateDefaultBlobRepository(new List<BlobInfo>());
            blobRepoMock.Setup(m => m.WriteBlobAsync(It.IsAny<string>(), It.IsAny<BlobStreamDecorator>(), It.IsAny<bool>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(apiExceptionType, exCtorParam));
            var optionsMock = new Mock<IOptionsSnapshot<FileStorageOptions>>();
            optionsMock.Setup(m => m.Value).Returns(new FileStorageOptions());

            //Act
            var service = new FileService(blobRepoMock.Object, optionsMock.Object, m_LoggerMock.Object);
            Func<Task> act = () => service.WriteFileAsync("foo", null);

            //Assert
            var ex = await Assert.ThrowsAnyAsync<Exception>(act);
            Assert.False(typeof(ApiException).IsAssignableFrom(ex.GetType()));
        }

        #endregion

        #region CleanUpFilesAsync

        [Fact]
        public async Task CleanUpFilesAsync_DelectedCount_ReturnsProperCount()
        {
            //Arrange
            var stubOptions = new FileStorageOptions()
            {
                ServerFileUploadSubFolder = "RestApi"
            };
            var mockBlobs = new List<BlobInfo>()
            {
                GetFakeBlobInfo("/foo", DateTimeOffset.UtcNow),
                GetFakeBlobInfo("/bar", DateTimeOffset.UtcNow.AddDays(-1)),
                GetFakeBlobInfo("/baz", DateTimeOffset.UtcNow.AddDays(-1))
            };
            var blobRepoMock = CreateDefaultBlobRepository(mockBlobs);
            var optionsMock = new Mock<IOptionsSnapshot<FileStorageOptions>>();
            optionsMock.Setup(m => m.Value).Returns(stubOptions);

            //Act
            var service = new FileService(blobRepoMock.Object, optionsMock.Object, m_LoggerMock.Object);
            int deleted = await service.CleanUpFilesAsync(1);

            //Assert
            Assert.Equal(2, deleted);
        }

        [Fact]
        public async Task CleanUpFilesAsync_ExpirationDelay_DeleteOnlyExpired()
        {
            //Arrange
            var stubOptions = new FileStorageOptions()
            {
                ServerFileUploadSubFolder = "RestApi"
            };
            var mockBlob = GetFakeBlobInfo("/foo", DateTimeOffset.UtcNow);
            var mockBlobs = new List<BlobInfo>()
            {
                GetFakeBlobInfo("/bar", DateTimeOffset.UtcNow.AddDays(-1)),
                mockBlob,
                GetFakeBlobInfo("/baz", DateTimeOffset.UtcNow.AddDays(-1))
            };
            var blobRepoMock = CreateDefaultBlobRepository(mockBlobs);
            var optionsMock = new Mock<IOptionsSnapshot<FileStorageOptions>>();
            optionsMock.Setup(m => m.Value).Returns(stubOptions);

            //Act
            var service = new FileService(blobRepoMock.Object, optionsMock.Object, m_LoggerMock.Object);
            await service.CleanUpFilesAsync(1);

            //Assert
            Assert.Single(mockBlobs, mockBlob);
        }

        [Fact]
        public async Task CleanUpFilesAsync_IoTHubContainer_DeleteOnlyRestApiBlobs()
        {
            //Arrange
            var stubOptions = new FileStorageOptions()
            {
                DeviceFileUploadFolder = nameof(FileStorageOptions.DeviceFileUploadFolder),
                ServerFileUploadFolder = nameof(FileStorageOptions.ServerFileUploadFolder),
                ServerFileUploadSubFolder = "RestApi"
            };
            var mockBlob = GetFakeBlobInfo($"/{stubOptions.DeviceFileUploadFolder}/foo/ips/patate.txt", DateTimeOffset.UtcNow.AddDays(-1));
            var mockBlobs = new List<BlobInfo>()
            {
                mockBlob,
                GetFakeBlobInfo(stubOptions.DeviceFileUploadFolder, "bar", DateTimeOffset.UtcNow.AddDays(-1)),
                GetFakeBlobInfo(stubOptions.ServerFileUploadFolder, "baz", DateTimeOffset.UtcNow.AddDays(-1))
            };
            var blobRepoMock = CreateDefaultBlobRepository(mockBlobs);
            var optionsMock = new Mock<IOptionsSnapshot<FileStorageOptions>>();
            optionsMock.Setup(m => m.Value).Returns(stubOptions);

            //Act
            var service = new FileService(blobRepoMock.Object, optionsMock.Object, m_LoggerMock.Object);
            await service.CleanUpFilesAsync(1);

            //Assert
            Assert.Single(mockBlobs);
            Assert.Equal(mockBlob, mockBlobs.Single());
        }

        [Fact]
        public async Task CleanUpFilesAsync_RelayContainer_DeleteAnyBlobs()
        {
            //Arrange
            var stubOptions = new FileStorageOptions()
            {
                DeviceFileUploadFolder = nameof(FileStorageOptions.DeviceFileUploadFolder),
                ServerFileUploadFolder = nameof(FileStorageOptions.ServerFileUploadFolder),
                ServerFileUploadSubFolder = "RestApi"
            };
            var mockBlobs = new List<BlobInfo>()
            {
                GetFakeBlobInfo(stubOptions.DeviceFileUploadFolder, "foo", DateTimeOffset.UtcNow.AddDays(-1)),
                GetFakeBlobInfo(stubOptions.ServerFileUploadFolder, "bar", DateTimeOffset.UtcNow.AddDays(-1)),
                GetFakeBlobInfo($"/{stubOptions.ServerFileUploadFolder}/baz/patate/patate.txt", DateTimeOffset.UtcNow.AddDays(-1))
            };
            var blobRepoMock = CreateDefaultBlobRepository(mockBlobs);
            var optionsMock = new Mock<IOptionsSnapshot<FileStorageOptions>>();
            optionsMock.Setup(m => m.Value).Returns(stubOptions);

            //Act
            var service = new FileService(blobRepoMock.Object, optionsMock.Object, m_LoggerMock.Object);
            await service.CleanUpFilesAsync(1);

            //Assert
            Assert.Empty(mockBlobs);
        }

        #endregion

        private Mock<IBlobRepository> CreateDefaultBlobRepository(List<BlobInfo> seed)
        {
            var blobRepoMock = new Mock<IBlobRepository>(MockBehavior.Strict);
            blobRepoMock.Setup(m => m.ListBlobAsync(It.IsAny<string>()))
                .ReturnsAsync((string path) => seed.Where(b => string.IsNullOrEmpty(path) || b.Path.StartsWith($"/{path}")).ToList());
            blobRepoMock.Setup(m => m.DeleteBlobAsync(It.IsAny<string>()))
                .ReturnsAsync((string path) => seed.Remove(seed.FirstOrDefault(b => b.Path == path)));
            return blobRepoMock;
        }

        private BlobInfo GetFakeBlobInfo(string containerName, string deviceName, DateTimeOffset lastModified)
        {
            return new FakeBlobInfo($"/{containerName}/{deviceName}/RestApi/{System.IO.Path.GetRandomFileName()}", lastModified);
        }

        private BlobInfo GetFakeBlobInfo(string blobPath, DateTimeOffset lastModified)
        {
            return new FakeBlobInfo(blobPath, lastModified);
        }

        class FakeBlobInfo
            : BlobInfo
        {
            public FakeBlobInfo(string path, DateTimeOffset lastModified)
            {
                Path = path;
                LastModified = lastModified;
            }
        }
    }
}
