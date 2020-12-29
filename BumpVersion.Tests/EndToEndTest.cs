using System;
using Xunit;
using Docker.DotNet;

namespace BumpVersion.Tests
{
    public class EndToEndTest
    {
        [Fact]
        public void Test1()
        {
            /**
             DockerClient client = new DockerClientConfiguration(
    new Uri("npipe://./pipe/docker_engine"))
     .CreateClient();
            */
            DockerClient client = new DockerClientConfiguration(
       new Uri("unix:///var/run/docker.sock"))
        .CreateClient();

        }
    }
}
