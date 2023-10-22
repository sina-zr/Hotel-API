using FluentValidation;
using SharedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValidationLibrary;

public class BookARoomBodyValidator : AbstractValidator<BookingBodyModel>
{
    public BookARoomBodyValidator()
    {
        RuleFor(p => p.RoomTypeId).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(IsInRoomNumberRange).WithMessage("RoomTypeId is invalid!");

        RuleFor(p => p.StartDate).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(BeValidStartDate).WithMessage("CheckIn date was either invalid or too far.");

        RuleFor(p => p.StartDate).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must((model, startDate) => BeValidEndDate(model, startDate)).WithMessage("CheckOut date was either invalid or too far.");
    }

    protected bool IsInRoomNumberRange(int roomTypeId)
    {
        bool output = roomTypeId switch
        {
            1 => true,
            2 => true,
            3 => true,
            > 3 => false,
            < 1 => false
        };

        return output;
    }

    protected bool BeValidStartDate(DateTime startDate)
    {
        bool output = true;

        if (startDate.Subtract(DateTime.Now).Days > 365)
        {
            output = false;
        }
        if (startDate.Subtract(DateTime.Now).Days < 0)
        {
            output = false;
        }

        return output;
    }

    protected bool BeValidEndDate(BookingBodyModel model, DateTime startDate)
    {
        bool output = true;

        if (model != null)
        {
            DateTime endDate = model.EndDate; // Assuming EndDate is a property in BookingBodyModel

            if (endDate.Subtract(startDate).Days > 7)
            {
                output = false;
            }
            if (endDate.Subtract(startDate).Days < 1)
            {
                output = false;
            }
        }
        return output;
    }
}
