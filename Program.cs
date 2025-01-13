using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

// Represents a room type (e.g., Single Room, Double Room).
public class RoomType
{
    public string Code { get; set; }
    public string Description { get; set; }
}

// Represents a specific room in a hotel.
public class Room
{
    public string Type { get; set; }
    public string Id { get; set; }
}

// Represents a hotel with its rooms and room types.
public class Hotel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<RoomType> RoomTypes { get; set; }
    public List<Room> Rooms { get; set; }
}

// Represents a booking made by a guest.
public class Booking
{
    public string HotelId { get; set; }
    public string CheckIn { get; set; }
    public string CheckOut { get; set; }
    public string RoomType { get; set; }
    public string Rate { get; set; }
}

public class Program
{
    // Loads hotel data from a JSON file.
    public static List<Hotel> LoadHotels(string filePath)
    {
        return JsonSerializer.Deserialize<List<Hotel>>(File.ReadAllText(filePath));
    }

    // Loads booking data from a JSON file.
    public static List<Booking> LoadBookings(string filePath)
    {
        return JsonSerializer.Deserialize<List<Booking>>(File.ReadAllText(filePath));
    }

    // Checks room availability for a given hotel, room type, and date range.
    public static int GetRoomAvailability(List<Hotel> hotels, List<Booking> bookings, string hotelId, string roomType, DateTime startDate, DateTime endDate)
    {
        var hotel = hotels.Find(h => h.Id == hotelId);
        if (hotel == null) return 0; // Hotel not found.

        int totalRooms = hotel.Rooms.Count(r => r.Type == roomType);
        int bookedRooms = bookings.Count(b =>
            b.HotelId == hotelId &&
            b.RoomType == roomType &&
            DateTime.ParseExact(b.CheckIn, "yyyyMMdd", null) < endDate &&
            DateTime.ParseExact(b.CheckOut, "yyyyMMdd", null) > startDate);

        return totalRooms - bookedRooms; // Available rooms.
    }

    // Main program entry point.
    public static void Main(string[] args)
    {
        if (args.Length < 4)
        {
            Console.WriteLine("Usage: myapp --hotels <hotels.json> --bookings <bookings.json>");
            return;
        }

        var hotels = LoadHotels(args[1]); // Load hotels data.
        var bookings = LoadBookings(args[3]); // Load bookings data.

        while (true)
        {
            Console.Write("Enter query (e.g., 'Availability(H1, 20240901, SGL)'): ");
            string input = Console.ReadLine();
            if (string.IsNullOrEmpty(input)) break; // Exit on empty input.

            var parts = input.Split(new[] { '(', ')', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4) continue; // Skip invalid input.

            string hotelId = parts[1];
            string[] dateRange = parts[2].Split('-');
            DateTime startDate = DateTime.ParseExact(dateRange[0], "yyyyMMdd", null);
            DateTime endDate = dateRange.Length > 1 ? DateTime.ParseExact(dateRange[1], "yyyyMMdd", null) : startDate.AddDays(1);
            string roomType = parts[3];

            Console.WriteLine(GetRoomAvailability(hotels, bookings, hotelId, roomType, startDate, endDate)); // Display availability.
        }
    }
}