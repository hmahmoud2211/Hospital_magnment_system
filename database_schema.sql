CREATE DATABASE hospital_management;
USE hospital_management;

CREATE TABLE Patients (
    PatientID INT PRIMARY KEY AUTO_INCREMENT,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Gender VARCHAR(10),
    Address VARCHAR(255),
    City VARCHAR(50),
    State VARCHAR(50),
    PostalCode VARCHAR(10),
    Phone VARCHAR(15),
    Email VARCHAR(100),
    EmergencyContact VARCHAR(100),
    EmergencyPhone VARCHAR(15),
    BloodGroup VARCHAR(5),
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE Doctors (
    DoctorID INT PRIMARY KEY AUTO_INCREMENT,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Specialization VARCHAR(100),
    Phone VARCHAR(15),
    Email VARCHAR(100),
    LicenseNumber VARCHAR(50),
    Department VARCHAR(50),
    JoinDate DATE,
    Status VARCHAR(20)
);

CREATE TABLE Rooms (
    RoomID INT PRIMARY KEY AUTO_INCREMENT,
    RoomNumber VARCHAR(10) NOT NULL,
    RoomType ENUM('Regular', 'ICU', 'Operation') NOT NULL,
    Floor INT,
    Status ENUM('Available', 'Occupied', 'Maintenance') DEFAULT 'Available',
    PricePerDay DECIMAL(10,2)
);

CREATE TABLE Appointments (
    AppointmentID INT PRIMARY KEY AUTO_INCREMENT,
    PatientID INT,
    DoctorID INT,
    AppointmentDate DATETIME,
    Status ENUM('Scheduled', 'Completed', 'Cancelled') DEFAULT 'Scheduled',
    PaymentStatus ENUM('Pending', 'Paid', 'Refunded') DEFAULT 'Pending',
    Amount DECIMAL(10,2),
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
    FOREIGN KEY (DoctorID) REFERENCES Doctors(DoctorID)
);

CREATE TABLE RoomAllocations (
    AllocationID INT PRIMARY KEY AUTO_INCREMENT,
    PatientID INT,
    RoomID INT,
    DoctorID INT,
    AdmissionDate DATETIME,
    DischargeDate DATETIME,
    Status ENUM('Active', 'Discharged') DEFAULT 'Active',
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
    FOREIGN KEY (RoomID) REFERENCES Rooms(RoomID),
    FOREIGN KEY (DoctorID) REFERENCES Doctors(DoctorID)
);

CREATE TABLE Treatments (
    TreatmentID INT PRIMARY KEY AUTO_INCREMENT,
    PatientID INT,
    DoctorID INT,
    Diagnosis TEXT,
    Treatment TEXT,
    StartDate DATE,
    EndDate DATE,
    Notes TEXT,
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
    FOREIGN KEY (DoctorID) REFERENCES Doctors(DoctorID)
);

CREATE TABLE Bills (
    BillID INT PRIMARY KEY AUTO_INCREMENT,
    PatientID INT,
    RoomAllocationID INT,
    BillDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    RoomCharges DECIMAL(10,2),
    TreatmentCharges DECIMAL(10,2),
    MedicineCharges DECIMAL(10,2),
    OtherCharges DECIMAL(10,2),
    TotalAmount DECIMAL(10,2),
    PaymentStatus ENUM('Pending', 'Paid', 'Partial') DEFAULT 'Pending',
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
    FOREIGN KEY (RoomAllocationID) REFERENCES RoomAllocations(AllocationID)
);

CREATE TABLE HospitalLocations (
    LocationID INT PRIMARY KEY AUTO_INCREMENT,
    HospitalName VARCHAR(100),
    Address VARCHAR(255),
    City VARCHAR(50),
    State VARCHAR(50),
    PostalCode VARCHAR(10),
    Latitude DECIMAL(10,8),
    Longitude DECIMAL(11,8),
    ContactNumber VARCHAR(15),
    EmailAddress VARCHAR(100)
); 