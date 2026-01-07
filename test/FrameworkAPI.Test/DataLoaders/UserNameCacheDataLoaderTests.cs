using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using FrameworkAPI.DataLoaders;
using Microsoft.AspNetCore.Http;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.Supervisor.Client;
using Xunit;

namespace FrameworkAPI.Test.DataLoaders;

public class UserNameCacheDataLoaderTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());

    private readonly Mock<ISupervisorHttpClient> _supervisorHttpClient = new();

    [Fact]
    public async Task LoadSingleAsync_With_Client_Returning_Error_Should_Return_Error()
    {
        // Arrange
        var userId = _fixture.Create<string>();

        _supervisorHttpClient
            .Setup(mock => mock.ResolveNames(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<User>(StatusCodes.Status500InternalServerError, "Error"));

        var dataLoader = new UserNameCacheDataLoader(_supervisorHttpClient.Object);

        // Act
        var result = await dataLoader.LoadAsync(userId, CancellationToken.None);

        // Assert
        result.Exception.Should().NotBeNull();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task LoadSingleAsync_Should_Return_Success()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var user = _fixture.Create<User>();

        _supervisorHttpClient
            .Setup(mock => mock.ResolveNames(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<User>([user]));

        var dataLoader = new UserNameCacheDataLoader(_supervisorHttpClient.Object);

        // Act
        var result = await dataLoader.LoadAsync(userId, CancellationToken.None);

        // Assert
        result.Exception.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value.Should().Be(user.Name);
    }
}