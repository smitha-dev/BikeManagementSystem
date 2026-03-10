INSERT INTO Students (StudentID, FirstName, LastName, Email) VALUES
('U1000001','Alex','Rivera','alex.rivera@univ.edu'),
('U1000002','Jordan','Lee','jordan.lee@univ.edu'),
('U1000003','Taylor','Nguyen','taylor.nguyen@univ.edu'),
('U1000004','Morgan','Patel','morgan.patel@univ.edu'),
('U1000005','Casey','Garcia','casey.garcia@univ.edu');

INSERT INTO Bikes (BikeID, Brand, Size, SeatHeight, Color, Status, DateAdded, LastUpdated) VALUES
(1,'Trek','M',30.5,'Red','Available','2024-01-10','2024-01-10'),
(2,'Giant','L',32.0,'Blue','Rented','2024-02-05','2024-03-01'),
(3,'Specialized','S',28.5,'Black','Maintenance','2024-02-15','2024-03-05'),
(4,'Cannondale','M',30.0,'Green','Available','2024-03-01','2024-03-01'),
(5,'Schwinn','L',31.5,'Silver','Rented','2024-03-10','2024-03-10');

INSERT INTO Maintenance (MaintenanceID, BikeID, DateFlagged, DateFixed, Notes, Cost) VALUES
(1,3,'2024-03-01',NULL,'Brake cable worn',25.00),
(2,1,'2024-02-20','2024-02-22','Flat tire replaced',15.50),
(3,5,'2024-03-11',NULL,'Gear slipping',40.00);

INSERT INTO Photos (PhotoID, BikeID, MaintenanceID, FilePath, PhotoType) VALUES
(1,1,NULL,'/photos/bikes/bike1.jpg','Bike'),
(2,2,NULL,'/photos/bikes/bike2.jpg','Bike'),
(3,3,1,'/photos/maintenance/brake_issue.jpg','Maintenance'),
(4,5,3,'/photos/maintenance/gear_problem.jpg','Maintenance'),
(5,4,NULL,'/photos/bikes/bike4.jpg','Bike');

INSERT INTO Rentals (
RentalID, BikeID, StudentID, SemesterRented, Year,
CheckoutDate, DueDate, ReturnDate,
CheckinDate1, CheckinDate2, CheckinDate3
) VALUES
(1,2,'U1000001','Spring',2025,'2025-01-15','2025-05-10',NULL,'2025-02-10',NULL,NULL),
(2,5,'U1000002','Spring',2025,'2025-01-20','2025-05-10',NULL,'2025-02-15','2025-03-01',NULL),
(3,1,'U1000003','Fall',2024,'2024-08-25','2024-12-15','2024-12-10','2024-09-20','2024-10-20','2024-11-20'),
(4,4,'U1000004','Spring',2025,'2025-02-01','2025-05-10',NULL,NULL,NULL,NULL);