﻿using Microsoft.AspNetCore.Mvc;
using workshop.wwwapi.Models;
using workshop.wwwapi.Repository;
using workshop.wwwapi.DTO;
namespace workshop.wwwapi.Endpoints
{
    public static class AppointmentEndpoint
    {
        public static void ConfigureAppointmentEndpoint(this WebApplication app)
        {
            var surgeryGroup = app.MapGroup("surgery");
            surgeryGroup.MapPost("/appointments", CreateAppointment);
            surgeryGroup.MapGet("/appointments", GetAllAppointments);
            surgeryGroup.MapGet("/appointments/patient/{patientId}", GetAppointmentByPatientId);
            surgeryGroup.MapGet("/appointments/doctor/{doctorId}", GetAppointmentByDoctorId);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        public static async Task<IResult> CreateAppointment(CreateAppointmentPayload payload, IRepository repository)
        {
            if (payload.Booking == null)
            {
                return TypedResults.BadRequest("Booking is required");
            }
            if (payload.DoctorId == null)
            {
                return TypedResults.BadRequest("DoctorId is required");
            }
            if (payload.PatientId == null)
            {
                return TypedResults.BadRequest("PatientId is required");
            }
            Doctor? doctor = await repository.GetDoctorById(payload.DoctorId, PreloadPolicy.DoNotPreloadRelations);
            if (doctor == null)
            {
                return TypedResults.BadRequest("Doctor does not exist");
            }
            Patient? patient = await repository.GetPatientById(payload.PatientId, PreloadPolicy.DoNotPreloadRelations);
            if (patient == null)
            {
                return TypedResults.BadRequest("Patient does not exist");
            }
            Prescription? prescription = await repository.GetPrescriptionById(payload.PrescriptionId, PreloadPolicy.DoNotPreloadRelations);
            if (prescription == null)
            {
                return TypedResults.BadRequest("Prescription does not exist");
            }
            Appointment? appointment = await repository.CreateAppointment(payload.Booking, doctor, patient, prescription, payload.Type);
            if (appointment == null)
            {
                return TypedResults.BadRequest("Appointment could not be created");
            }
            return TypedResults.Ok(new AppointmentDTO(appointment));
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        public static async Task<IResult> GetAllAppointments(IRepository repository)
        {
            var appointments = await repository.GetAppointments();
            var AppointmentDTO = new List<AppointmentDTO>();
            foreach (Appointment appointment in appointments)
            {
                AppointmentDTO.Add(new AppointmentDTO(appointment));
            }
            return TypedResults.Ok(AppointmentDTO);
        }


        public static async Task<IResult> GetAppointmentByPatientId(IRepository repository, int patientId)
        {
            var appointment = await repository.GetAppointmentByPatientId(patientId);
            var appointmentDTO = new AppointmentDTO(appointment);
            return TypedResults.Ok(appointmentDTO);
        }

        
        public static async Task<IResult> GetAppointmentByDoctorId(IRepository repository, int doctorId)
        {
            var appointment = await repository.GetAppointmentByDoctorId(doctorId);
            var appointmentDTO = new AppointmentDTO(appointment);
            return TypedResults.Ok(appointmentDTO);
        }

        public static async Task<IResult> LinkPrescriptionToAppointment(IRepository repository, int prescriptionId, int doctorId, int patientId)
        {
            if (prescriptionId.Equals(null)) {
                return TypedResults.BadRequest("PrescriptionId is required");
            }
            if (doctorId.Equals(null)) {
                return TypedResults.BadRequest("DoctorId is required");
            }
            if (patientId.Equals(null)) {
                return TypedResults.BadRequest("PatientId is required");
            }
            Doctor? doctor = await repository.GetDoctorById(doctorId, PreloadPolicy.DoNotPreloadRelations);
            if (doctor == null)
            {
                return TypedResults.BadRequest("Doctor does not exist");
            }
            Patient? patient = await repository.GetPatientById(patientId, PreloadPolicy.DoNotPreloadRelations);
            if (patient == null)
            {
                return TypedResults.BadRequest("Patient does not exist");
            }
            Prescription? prescription = await repository.GetPrescriptionById(prescriptionId, PreloadPolicy.DoNotPreloadRelations);
            if (prescription == null)
            {
                return TypedResults.BadRequest("Prescription does not exist");
            }
            Appointment? appointment = await repository.GetAppointmentByDoctorId(doctorId);
            if (appointment == null)
            {
                return TypedResults.BadRequest("Appointment does not exist");
            }

            var link = await repository.LinkPrescriptionToAppointment(prescriptionId, doctorId, patientId);
            return TypedResults.Ok(new AppointmentDTO(link));
        }
    }
}