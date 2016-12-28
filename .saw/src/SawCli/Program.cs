﻿namespace Microsoft.Ciqs.Saw.Cli
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Ciqs.Saw.Common;
    using Microsoft.Ciqs.Saw.Phases;
    using Microsoft.WindowsAzure.Storage;

    class Program
    {
        private static void PrintUsage(bool isInvalid = false)
        {
            if (isInvalid)
            {
                Console.WriteLine("Invalid command.");
            }
        }
        
        
        static int Main(string[] args)
        {
            var phaseList = (new PhaseListProvider(args)).Phases;
            
            var clArgsParser = new CommandLineArgumentsParser(args);
       
            var root = Path.GetFullPath(Environment.GetEnvironmentVariable("SAW_ROOT"));
            var solutionsRoot = Path.GetFullPath(Path.Combine(root, ConfigurationManager.AppSettings["SolutionsDirectory"]));
            
            var command = args[0];
            
            Action buildLambda = () => {
                var packagesDirectory = Path.GetFullPath(Environment.GetEnvironmentVariable("PACKAGES_DIRECTORY"));
                SolutionBuilderPhase builder = new SolutionBuilderPhase(solutionsRoot, packagesDirectory);
                builder.Build();                
            };
            
            Action deployLambda = () => {
                string storageConnectionString = ConfigurationManager.AppSettings["SolutionStorageConnectionString"];
                CloudStorageAccount account = CloudStorageAccount.Parse(storageConnectionString);
                SolutionDeployerPhase deployer = new SolutionDeployerPhase(solutionsRoot, account);
                deployer.Deploy();                                    
            };
            
            switch (clArgsParser.Command)
            {
                case "build":
                    buildLambda();
                    break;
                case "deploy":
                    buildLambda();
                    deployLambda();
                    break;
                default:
                    Program.PrintUsage(true);
                    return 1;
            }
            
            return 0;
        }
    }
}
