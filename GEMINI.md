# Family Memories Project Overview

This project is an ASP.NET Core web application designed to store and display family memories, primarily in the form of images with associated titles, descriptions, and dates. It leverages Entity Framework Core for data persistence and ASP.NET Core Identity for user authentication and authorization.

## Key Features:
- **Memory Management:** Users can create, view, edit, and delete family memories. Each memory includes an image, title, description, date, and is associated with a user.
- **User Authentication:** Secure user login and registration using ASP.NET Core Identity.
- **Dynamic Home Page:**
    - **Not Logged In:** All memories are displayed in a gallery format. Users can click on a memory to view details in a modal.
    - **Logged In:** In addition to viewing all memories, authenticated users can add new memories and see an "Edit" button on each memory card they have permission to edit. Clicking a memory still opens the detail modal.
- **Image Uploads:** Supports uploading images for memories.
- **Responsive Design:** The gallery and modal views are designed to be responsive across different screen sizes.

## Technologies Used:
- **Backend:** ASP.NET Core 8.0 (C#)
- **Database:** SQLite (via Entity Framework Core)
- **Frontend:** HTML, CSS (gallery.css), JavaScript (jQuery)
- **Authentication:** ASP.NET Core Identity
- **UI Framework:** Bootstrap (for some basic styling, though custom CSS is prominent)

## Project Structure:
- `Controllers/`: Contains MVC controllers, including `HomeController` for the main gallery and `Api/MemoriesController` for API endpoints.
- `Data/`: Entity Framework Core `DbContext`, database initializer, and migrations.
- `Models/`: Data models (`Memory.cs`, `ApplicationUser.cs`, `ApplicationRole.cs`) and ViewModels.
- `Pages/`: Razor Pages for Identity management (Login, Register, Logout) and CRUD operations for Memories.
- `Views/`: MVC views, primarily `Home/Index.cshtml` for the main gallery.
- `wwwroot/`: Static files like CSS, JavaScript, and image uploads.

## Building and Running:

### Prerequisites:
- .NET SDK 8.0 or later
- A code editor (e.g., Visual Studio, VS Code)

### Commands:

1.  **Restore Dependencies:**
    ```bash
    dotnet restore
    ```

2.  **Apply Database Migrations:**
    ```bash
    dotnet ef database update
    ```
    *(Note: If you make changes to the data models, you'll need to create a new migration first: `dotnet ef migrations add [MigrationName]`)*

3.  **Run the Application:**
    ```bash
    dotnet run
    ```
    The application will typically run on `https://localhost:7000` or `http://localhost:5000`.

## Development Conventions:
- **MVC Pattern:** The application follows the Model-View-Controller pattern for its core logic.
- **Razor Pages for Identity:** ASP.NET Core Identity features are implemented using Razor Pages.
- **Entity Framework Core:** Used for all database interactions.
- **Client-side Scripting:** jQuery is used for dynamic client-side interactions, especially for the gallery and modal.
- **CSS Styling:** Custom CSS in `wwwroot/css/gallery.css` defines the visual style of the memory gallery.

## Future Enhancements:
- Implement filtering by user for memories.
- Add more robust error handling and user feedback.
- Improve UI/UX for memory creation and editing.
