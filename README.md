# LMS-backend

## Overview

**LMS-backend** is a backend service for a Library Management System (LMS) built in C#. It provides APIs to manage books, authors, publishers, reviews, users, orders, carts, and announcements. The system is designed for functionality such as book management, inventory control, user carts, order processing, and review moderation.

## Features

- **Book Management:** Create, update, delete, and retrieve books with details like authors, publishers, genres, formats, languages, stock, and discounts.
- **Author & Publisher Management:** Add, update, remove, and view authors and publishers, and retrieve books associated with them.
- **User Carts & Orders:** Add to cart, update/remove items, view cart summary, clear cart, and process orders.
- **Book Reviews:** Users can add reviews for books they've purchased, and view reviews by book or user.
- **Announcements:** Manage and retrieve library announcements.
- **Book Availability:** Track stock, new arrivals, best sellers, and coming soon titles.
- **Discounts & Deals:** Set and remove discounts on books, and retrieve current deals.

## Technology Stack

- **Language:** C#
- **Framework:** ASP.NET Core (API)
- **Database:** Entity Framework Core (with support for various providers)
- **ORM:** Entity Framework Core
- **Other:** AutoMapper, Cloudinary (for image uploads)

## Main Entities

- **User**
- **Book**
- **Author**
- **Publisher**
- **Order** / **OrderItem**
- **Cart** / **CartItem**
- **Review**
- **Announcement**

## API Highlights

- CRUD operations for books, authors, publishers.
- Cart operations: add, update, remove, summary, and clear.
- Order creation and management.
- Review creation and retrieval, restricted to users who’ve purchased the book.
- Book availability: new releases, new arrivals, coming soon, deals, best sellers.
- Support for discounts, stock management, and cover image updates.

## Getting Started

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/DoCurry/LMS-backend.git
   ```

2. **Install Dependencies:**
   - Ensure you have [.NET SDK](https://dotnet.microsoft.com/download) installed.
   - Restore NuGet packages:
     ```bash
     dotnet restore
     ```

3. **Configure Environment:**
   - Set up your database connection string in `appsettings.json`.
   - Configure Cloudinary and other third-party service credentials as needed.

4. **Apply Migrations:**
   ```bash
   dotnet ef database update
   ```

5. **Run the Application:**
   ```bash
   dotnet run
   ```

## Project Structure

- `Controllers/` - API endpoints for managing resources.
- `Services/` - Business logic for books, carts, orders, reviews, authors, publishers, etc.
- `Dtos/` - Data Transfer Objects for API communication.
- `Entities/` - Database models.
- `Data/` - Database context.

## Contributing

1. Fork the repository.
2. Create a feature branch.
3. Commit your changes.
4. Submit a pull request.

## License

This project is licensed under the MIT License.

---

*For questions or support, please open an issue on the repository.*
