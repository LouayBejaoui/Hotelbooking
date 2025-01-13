using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

// This class is like a blueprint for what a room type looks like.
public class RoomType
{
    public string Code { get; set; } // Short name for the room type, like "SGL" for Single.
    public string Description { get; set; } // A friendly description, like "Single Room".
}

// This class represents a specific room in a hotel.
public class Room
{
    public string Type { get; set; } // The type of room, like "SGL" or "DBL".
    public string Id { get; set; } // The room number, like "101".
}

// This class holds all the details about a hotel.
public class Hotel
{
    public string Id { get; set; } // The hotel's unique ID, like "H1".
    public string Name { get; set; } // The hotel's name, like "Hotel California".
    public List<RoomType> RoomTypes { get; set; } // A list of room types the hotel offers.
    public List<Room> Rooms { get; set; } // A list of all the rooms in the hotel.
}

// This class represents a booking made by a guest.
public class Booking
{
    public string HotelId { get; set; } // The ID of the hotel where the booking is made.
    public string CheckIn { get; set; } // The check-in date, like "20240901".
    public string CheckOut { get; set; } // The check-out date, like "20240903".
    public string RoomType { get; set; } // The type of room booked, like "SGL".
    public string Rate { get; set; } // The rate type, like "Prepaid" or "Standard".
}

public class Program
{
    // This method reads the list of hotels from a JSON file.
    public static List<Hotel> LoadHotels(string filePath)
    {
        string json = File.ReadAllText(filePath); // Read the file into a string.
        return JsonSerializer.Deserialize<List<Hotel>>(json); // Turn the JSON into a list of hotels.
    }

    // This method reads the list of bookings from a JSON file.
    public static List<Booking> LoadBookings(string filePath)
    {
        string json = File.ReadAllText(filePath); // Read the file into a string.
        return JsonSerializer.Deserialize<List<Booking>>(json); // Turn the JSON into a list of bookings.
    }

    // This method checks how many rooms are available for a given hotel, room type, and date range.
    public static int GetRoomAvailability(List<Hotel> hotels, List<Booking> bookings, string hotelId, string roomType, DateTime startDate, DateTime endDate)
    {
        // Find the hotel by its ID.
        Hotel hotel = hotels.Find(h => h.Id == hotelId);
        if (hotel == null) return 0; // If the hotel doesn't exist, return 0.

        // Count how many rooms of the requested type are in the hotel.
        int totalRooms = hotel.Rooms.Count(r => r.Type == roomType);

        // Count how many rooms of this type are already booked for the given dates.
        int bookedRooms = bookings.Count(b =>
            b.HotelId == hotelId &&
            b.RoomType == roomType &&
            DateTime.ParseExact(b.CheckIn, "yyyyMMdd", null) < endDate &&
            DateTime.ParseExact(b.CheckOut, "yyyyMMdd", null) > startDate);

        // Calculate availability: total rooms minus booked rooms.
        return totalRooms - bookedRooms;
    }

    // This is where the program starts running.
    public static void Main(string[] args)
    {
        // Check if the user provided the required file paths.
        if (args.Length < 4)
        {
            Console.WriteLine("Usage: myapp --hotels <hotels.json> --bookings <bookings.json>");
            return;
        }

        // Get the file paths from the command-line arguments.
        string hotelsFile = args[1];
        string bookingsFile = args[3];

        // Load the hotels and bookings from the JSON files.
        List<Hotel> hotels = LoadHotels(hotelsFile);
        List<Booking> bookings = LoadBookings(bookingsFile);

        // Keep asking the user for input until they press Enter without typing anything.
        while (true)
        {
            Console.Write("Enter a query like 'Availability(H1, 20240901, SGL)' or 'Availability(H1, 20240901-20240903, DBL)': ");
            string input = Console.ReadLine();

            // If the user just presses Enter, exit the program.
            if (string.IsNullOrEmpty(input)) break;

            // Split the input into parts to extract the hotel ID, date range, and room type.
            string[] parts = input.Split(new[] { '(', ')', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4) continue; // Skip if the input isn't in the right format.

            string hotelId = parts[1]; // The hotel ID, like "H1".
            string[] dateRange = parts[2].Split('-'); // The date range, like "20240901-20240903".
            string roomType = parts[3]; // The room type, like "SGL".

            // Parse the start and end dates.
            DateTime startDate = DateTime.ParseExact(dateRange[0], "yyyyMMdd", null);
            DateTime endDate = dateRange.Length > 1 ? DateTime.ParseExact(dateRange[1], "yyyyMMdd", null) : startDate.AddDays(1);

            // Check how many rooms are available and display the result.
            int availability = GetRoomAvailability(hotels, bookings, hotelId, roomType, startDate, endDate);
            Console.WriteLine(availability);
        }
    }
}