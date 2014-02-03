﻿namespace Lacjam.ServiceBus

    open System
    open Autofac
    open NServiceBus
    open NServiceBus.Features
    open Lacjam.Core
    open Lacjam.Core.Runtime
    open Lacjam.Core.Scheduling
    open Lacjam.Core.Jobs
    open Lacjam.Core.Settings
    open Lacjam.Integration
    open Quartz
    open Quartz.Impl
    open Quartz.Core

    type StartupBatchJobs() =
   
//        let j1 = PageScraperJob(Payload="http://www.bedlam.net.au") :> JobMessage
//        let j2 = PageScraperJob(Payload="http://www.mckelt.com")  :> JobMessage
//        let j3 = PageScraperJob(Payload="http://www.mckelt.com/blog") :> JobMessage
//        let Batchs = seq [j1; j2; j3;]
       
    //    let pingBatches = {
    //        Batch.Id = Guid.NewGuid(); 
    //        Batch.Name = "site-wakeup" ; 
    //        Batch.Jobs = Batchs 
    //        Batch.RunOnSchedule =TimeSpan.FromMinutes(Convert.ToDouble(1))
    //        }

//        let scheduleJiraRoadmapOutput() =
//                                    let jiraJob = new CustomJobs.JiraRoadMapOutputJob() 
//                                    Schedule.Every(TimeSpan.FromMinutes(Convert.ToDouble(3))).Action(fun a->
//                                                                                                try
//                                                                                                    Lacjam.Core.Runtime.Ioc.Resolve<ILogWriter>().Write(LogMessage.Debug("Schedule running for JIRA Roadmap Job."))
//                                                                                                    Lacjam.Core.Runtime.Ioc.Resolve<IBus>().Send("lacjam.servicebus", jiraJob :> IMessage) |> ignore
//                                                                                                with 
//                                                                                                | ex ->  Lacjam.Core.Runtime.Ioc.Resolve<ILogWriter>().Write(LogMessage.Error("Schedule ACTION startup:",ex, true)) 
//                                    )
//                                    ()
    


        interface IContainBatches with
            override this.Batches = 
                                            let createG = Guid.NewGuid
                                            let guidId = createG()
                                            let swJob = CustomJobs.SwellNetRatingJob(Lacjam.Core.Runtime.Ioc.Resolve<ILogWriter>())
                                            let log = Ioc.Resolve<ILogWriter>()
                                            log.Write(Debug("StartupBatchJobs init"))

                                            //http://quartz-scheduler.org/documentation/quartz-1.x/tutorials/crontrigger
                                            //let trig = TriggerBuilder.Create().WithSchedule(CronScheduleBuilder.CronSchedule("0 0/5 5 * * ?").WithMisfireHandlingInstructionFireAndProceed()).StartNow()
                                            //let trig = TriggerBuilder.Create().WithSimpleSchedule(fun a-> a.WithIntervalInSeconds(30)|>ignore).StartNow().Build()
                                            //let trig = TriggerBuilder.Create().WithSchedule(CronScheduleBuilder.CronSchedule("0 0/5 5 * * ?").WithMisfireHandlingInstructionFireAndProceed()).StartNow().Build()
                                            let trig =  TriggerBuilder.Create().WithDailyTimeIntervalSchedule(fun a->a.WithIntervalInHours(24).StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(5, 15)).InTimeZone(TimeZoneInfo.Local) |> ignore).Build()
    
//                                            let trig = new Quartz.Impl.Triggers.DailyTimeIntervalTriggerImpl()
//                                            trig.Name <- "trig-daily " + Guid.NewGuid().ToString()
//                                            let startTomorrow =  Quartz.DateBuilder.TomorrowAt(5,20,0)
//                                            trig.StartTimeUtc <- startTomorrow
//                                            trig.StartTimeOfDay <- TimeOfDay.HourMinuteAndSecondOfDay(5, 30,0)
//                                            trig.RepeatIntervalUnit <- IntervalUnit.Day
//                                           // trig.RepeatInterval <- 24
//                                            //trig.RepeatCount <- 3
//                                            trig.EndTimeOfDay  <- TimeOfDay.HourMinuteAndSecondOfDay(6, 30,0)
//                                            trig.TimeZone <- TimeZoneInfo.Local
                                            let spi = trig :?> Spi.IOperableTrigger
                                            let times = TriggerUtils.ComputeFireTimes(spi, null,10)
                                            for time in times do
                                                 log.Write(Debug(time.ToLocalTime().ToString()))

                                            swJob.BatchId  <- guidId
                                            let swJobs = [
                                                                        StartUpJob(BatchId=guidId) :> JobMessage
                                                                        PageScraperJob(BatchId=guidId, Id=guidId, Url = "http://www.swellnet.com/reports/australia/new-south-wales/cronulla") :> JobMessage
                                                                        swJob :> JobMessage
                                                                        SendTweetJob(To="chris_mckelt") :> JobMessage
                                                                        //SendEmailJob(Email={To="Chris@mckelt.com";From="Chris@mckelt.com";Subject="SwellNet Rating: {0}";Body="SwellNet Rating: {0}"}) :> JobMessage
                                                         ]

                                            let surfReportBatch = {Batch.BatchId=guidId; Batch.CreatedDate=DateTime.UtcNow; Batch.Id=Guid.NewGuid(); Batch.Name="surfReportBatch";Batch.Jobs=swJobs; Batch.Status=BatchStatus.Waiting;Batch.TriggerBuilder=trig.GetTriggerBuilder();}
                                            //let jiraRoadmapBatch = {Batch.BatchId=Guid.NewGuid(); Batch.CreatedDate=DateTime.UtcNow; Batch.Id=Guid.NewGuid(); Batch.Name="jiraRoadmap";Batch.Jobs=[CustomJobs.JiraRoadMapOutputJob()]; Batch.Status=BatchStatus.Waiting;Batch.TriggerBuilder=trig.GetTriggerBuilder();}
                                            [surfReportBatch]
