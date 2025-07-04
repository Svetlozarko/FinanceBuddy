# 💰 FinanceTracker - Personal Financial Dashboard

<div align="center">

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-6.0+-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)
![JavaScript](https://img.shields.io/badge/JavaScript-F7DF1E?style=for-the-badge&logo=javascript&logoColor=black)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)



</div>

---

## 📋 Table of Contents

- [✨ Features](#-features)
- [🛠️ Technologies Used](#️-technologies-used)
- [🚀 Getting Started](#-getting-started)
- [📁 Project Structure](#-project-structure)
- [🎨 UI Components](#-ui-components)
- [🔐 Authentication](#-authentication)
- [📱 Responsive Design](#-responsive-design)
- [🎯 Usage](#-usage)
- [🤝 Contributing](#-contributing)
- [📄 License](#-license)

---

## ✨ Features

### 🏠 **Landing Page**
- **Modern Hero Section** with gradient backgrounds
- **Feature Showcase** highlighting key capabilities
- **Financial Articles** with expert tips and guides
- **Responsive Design** optimized for all devices
- **Call-to-Action** sections for user engagement

### 📊 **Dashboard**
- **Real-time Financial Metrics** with interactive charts
- **Transaction Management** with detailed history
- **Budget Tracking** and expense categorization
- **Visual Analytics** with Chart.js integration
- **Customizable Widgets** for personalized experience

### 🔐 **Authentication System**
- **Multi-step Registration** with progress indicators
- **Secure Login** with password strength validation
- **Social Authentication** (Google, Facebook)
- **Password Recovery** functionality
- **Remember Me** option for convenience

### 📚 **Content Management**
- **Financial Articles** with rich content
- **Image Management** with optimized loading
- **SEO-friendly URLs** for better discoverability
- **Article Categories** and tagging system
- **Reading time estimates**

### 🎨 **Modern UI/UX**
- **Gradient Backgrounds** with mesh patterns
- **Glass Morphism** effects on cards
- **Smooth Animations** and transitions
- **Dark/Light Theme** support
- **Accessibility** compliant design

---

## 🛠️ Technologies Used

### **Backend**
- **ASP.NET Core 6.0+** - Web framework
- **Entity Framework Core** - ORM for database operations
- **ASP.NET Core Identity** - Authentication and authorization
- **SQL Server** - Primary database
- **AutoMapper** - Object mapping

### **Frontend**
- **Razor Pages** - Server-side rendering
- **Bootstrap 5.3** - CSS framework
- **JavaScript ES6+** - Client-side functionality
- **Chart.js** - Data visualization
- **Font Awesome** - Icon library
- **Custom CSS** - Advanced styling and animations

### **Development Tools**
- **Visual Studio 2022** - IDE
- **Git** - Version control
- **npm** - Package management
- **Webpack** - Asset bundling

---

## 🚀 Getting Started

### **Prerequisites**

Before you begin, ensure you have the following installed:

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB is fine for development)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

### **Installation**

1. **Clone the repository**
   \`\`\`bash
   git clone https://github.com/Svetlozarko/FinanceTracker.git
   cd financetracker
   \`\`\`

2. **Restore NuGet packages**
   \`\`\`bash
   dotnet restore
   \`\`\`

3. **Update database connection string**
   
   Edit `appsettings.json`:
   \`\`\`json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FinanceTrackerDb;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   \`\`\`

4. **Apply database migrations**
   \`\`\`bash
   dotnet ef database update
   \`\`\`

5. **Build and run the application**
   \`\`\`bash
   dotnet build
   dotnet run
   \`\`\`

6. **Open your browser**
   
   Navigate to `https://localhost:5001` or `http://localhost:5000`

---

