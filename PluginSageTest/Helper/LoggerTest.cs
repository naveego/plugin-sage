using System;
using System.IO;
using PluginSage.Helper;
using Xunit;

namespace PluginSageTest.Helper
{
    public class LoggerTest
    {
        private static string _path = @"logs/plugin-sage-log.txt";
        
        [Fact]
        public void VerboseTest()
        {
            // setup
             try
            {
                File.Delete(_path);
            }
            catch (Exception e)
            {
            }
            Logger.SetLogLevel(Logger.LogLevel.Verbose);
            
            // act
            Logger.Verbose("verbose");
            Logger.Debug("debug");
            Logger.Info("info");
            Logger.Error(new Exception("error"), "error");

            // assert
            string[] lines = File.ReadAllLines(_path);

            Assert.Equal(4, lines.Length);
            
            // cleanup
             try
            {
                File.Delete(_path);
            }
            catch (Exception e)
            {
            }
        }
        
        [Fact]
        public void DebugTest()
        {
            // setup
            try
            {
                 try
            {
                File.Delete(_path);
            }
            catch (Exception e)
            {
            }
            }
            catch (Exception e)
            {
            }

            Logger.SetLogLevel(Logger.LogLevel.Debug);
            
            // act
            Logger.Verbose("verbose");
            Logger.Debug("debug");
            Logger.Info("info");
            Logger.Error(new Exception("error"), "error");

            // assert
            string[] lines = File.ReadAllLines(_path);

            Assert.Equal(3, lines.Length);
            
            // cleanup
             try
            {
                File.Delete(_path);
            }
            catch (Exception e)
            {
            }
        }
        
        [Fact]
        public void InfoTest()
        {
            // setup
             try
            {
                File.Delete(_path);
            }
            catch (Exception e)
            {
            }
            Logger.SetLogLevel(Logger.LogLevel.Info);
            
            // act
            Logger.Verbose("verbose");
            Logger.Debug("debug");
            Logger.Info("info");
            Logger.Error(new Exception("error"), "error");

            // assert
            string[] lines = File.ReadAllLines(_path);

            Assert.Equal(2, lines.Length);
            
            // cleanup
             try
            {
                 try
            {
                File.Delete(_path);
            }
            catch (Exception e)
            {
            }
            }
            catch (Exception e)
            {
            }
        }
        
        [Fact]
        public void ErrorTest()
        {
            // setup
             try
            {
                File.Delete(_path);
            }
            catch (Exception e)
            {
            }
            Logger.SetLogLevel(Logger.LogLevel.Error);
            
            // act
            Logger.Verbose("verbose");
            Logger.Debug("debug");
            Logger.Info("info");
            Logger.Error(new Exception("error"), "error");

            // assert
            string[] lines = File.ReadAllLines(_path);

            Assert.Single(lines);
            
            // cleanup
             try
            {
                File.Delete(_path);
            }
            catch (Exception e)
            {
            }
        }
        
        [Fact]
        public void OffTest()
        {
            // setup
             try
            {
                File.Delete(_path);
            }
            catch (Exception e)
            {
            }
            Logger.SetLogLevel(Logger.LogLevel.Off);
            
            // act
            Logger.Verbose("verbose");
            Logger.Debug("debug");
            Logger.Info("info");
            Logger.Error(new Exception("error"), "error");

            // assert
            string[] lines = File.Exists(_path) ? File.ReadAllLines(_path) : new string[0];

            Assert.Empty(lines);
            
            // cleanup
             try
            {
                File.Delete(_path);
            }
            catch (Exception e)
            {
            }
        }
    }
}