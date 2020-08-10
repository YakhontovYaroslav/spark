using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Threading;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Model;
using System.Collections.Generic;

namespace Spark.TestBench
{
    class Program : BackgroundService
    {
        private readonly IHostApplicationLifetime _applicationLifetime;

        static async System.Threading.Tasks.Task Main(string[] args) => 
            await CreateHostBuilder(args)
                  .RunConsoleAsync();

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddHostedService<Program>();
            });

        public Program(IHostApplicationLifetime applicationLifetime) => 
            _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));

        protected override System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken) =>
            System.Threading.Tasks.Task.Run(async () =>
            {
                await WorkAsync(stoppingToken);
                Console.WriteLine("Done. Press <ENTER> to shutdown.");
                Console.ReadLine();
                _applicationLifetime.StopApplication();
            }, stoppingToken);

        private async System.Threading.Tasks.Task WorkAsync(CancellationToken stoppingToken)
        {
            using var client = new FhirClient("https://localhost:5001/fhir");
            client.PreferredFormat = ResourceFormat.Json;
            client.PreferredReturn = Prefer.ReturnRepresentation;

            var location = new Location();
            location.Id = "1";
            location.Name = "Health Level Seven International";
            location.Description = "HL7 Headquarters";
            location.Status = Location.LocationStatus.Active;
            location.Mode = Location.LocationMode.Instance;
            location.PhysicalType = new CodeableConcept("http://terminology.hl7.org/CodeSystem/location-physical-type", "bu", "Building", "");
            location.Type = new List<CodeableConcept>
            {
                new CodeableConcept("http://terminology.hl7.org/CodeSystem/v3-RoleCode", "SLEEP", "Sleep disorders unit", "")
            };
            location.Position = new Location.PositionComponent() { Longitude = 42.256500m, Latitude = -83.694710m };
            location.Address = new Address()
            {
                Line = new List<string> { "3300 Washtenaw Avenue, Suite 227" },
                City = "Ann Arbor",
                State = "MI",
                PostalCode = "48104",
                Country = "USA"
            };
            location.Telecom = new List<ContactPoint>
            {
                new ContactPoint(ContactPoint.ContactPointSystem.Phone, null, "(+1) 734-677-7777"),
                new ContactPoint(ContactPoint.ContactPointSystem.Fax, null, "(+1) 734-677-6622"),
                new ContactPoint(ContactPoint.ContactPointSystem.Email, null, "hq@HL7.org"),
            };

            await client.ValidateCreateAsync(location);
            await client.CreateAsync(location);

            //Hl7.Fhir.Utility.
            var sched = new Schedule();
            sched.Id = "1";
            sched.Active = true;
            sched.Comment = "Bla bla";
            sched.PlanningHorizon = new Period(new FhirDateTime(DateTimeOffset.Now), new FhirDateTime(DateTimeOffset.Now.AddDays(7)));
            sched.ServiceCategory = new List<CodeableConcept>
            {
                new CodeableConcept("", "17", "General Practice", "")
            };
            sched.ServiceType = new List<CodeableConcept>
            {
                new CodeableConcept("", "57", "Immunization", "")
            };
            sched.Specialty = new List<CodeableConcept>
            {
                new CodeableConcept("", "408480009", "Clinical immunology", "")
            };
            sched.Actor = new List<ResourceReference>
            {
                new ResourceReference("Location/1")
            };

            await client.ValidateCreateAsync(sched);
            await client.CreateAsync(sched);
        }
    }
}
