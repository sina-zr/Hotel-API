using EFDataAccessLibrary.DataAccess;
using EFDataAccessLibrary.Models;
using FluentValidation;
using SharedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValidationLibrary
{
    public class AddRoomValidation : AbstractValidator<AddRoomModel>
    {
        private readonly IHotelContext _db;

        public AddRoomValidation(IHotelContext db)
        {
            _db = db;

            RuleFor(p => p.RoomTypeId).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(RoomTypeIdExists).WithMessage("The RoomTypeId you passed doesn't exist.");

            RuleFor(p => p.RoomNumber).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(RoomNumberNotRepetitive).WithMessage("RoomNumber you selected is already used.");
        }

        protected bool RoomTypeIdExists(int roomTypeId)
        {
            var RoomTypeIDs = _db.RoomTypes
                .Select(t => t.Id);

            return RoomTypeIDs.Contains(roomTypeId);
        }

        protected bool RoomNumberNotRepetitive(int roomNumber)
        {
            var RoomNumbers = _db.Rooms
                .Select(r => r.RoomNumber);

            return !RoomNumbers.Contains(roomNumber);
        }
    }
}
