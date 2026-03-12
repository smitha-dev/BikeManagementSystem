--View all bikes
SELECT *
FROM Bikes;

--View all students
SELECT *
FROM Students;

--View all rentals
SELECT *
FROM Rentals;

--View all maintenance records
SELECT *
FROM Maintenance;

--Show bikes that need repair
SELECT BikeID, Status,  LastUpdated
FROM Bikes
WHERE Status = "Maintenance"
ORDER BY BikeID;

--Show Overdue Rentals
SELECT RentalID, BikeID, StudentID, DueDate
FROM Rentals
WHERE DueDate < CURRENT_DATE
AND ReturnDate IS NULL;

--Show Bikes Currently Rented
SELECT BikeID, Brand, Color
FROM Bikes
WHERE Status = 'Rented';

--View Rental History for a Bike
SELECT RentalID, StudentID, SemesterRented, Year, CheckoutDate, ReturnDate
FROM Rentals
WHERE BikeID = 1;

--Show All Available Bikes
SELECT BikeID, Brand, Size, Color
FROM Bikes
WHERE Status = 'Available';



