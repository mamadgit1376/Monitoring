🎨 Frontend README: React & Next.js
This file should be named README.md and placed in the root of your Next.js project folder.

Proactive API Monitoring & Alerting System - Frontend
This repository contains the frontend for the Proactive API Monitoring System. It is a modern, responsive, and fast web application built with React and Next.js 15.

This frontend provides the user interface for interacting with the monitoring system. All data fetching is handled server-side (SSR) for optimal performance and SEO. It communicates with the backend via RESTful API calls to manage companies, endpoints, and display status information.

Technology Stack
Framework: Next.js 15

Library: React

Styling: Tailwind CSS / CSS Modules (or your choice)

Data Fetching: Server-Side Rendering (SSR) with fetch API

Core Features
Interactive Dashboard: A central dashboard that provides a real-time, at-a-glance view of the status (UP / DOWN) of all monitored services.

Company & Endpoint Management: User-friendly forms and tables for adding, viewing, and deleting companies and their associated API endpoints to be monitored.

Server-Side Rendering (SSR): All API data is fetched on the server before the page is sent to the client, resulting in fast page loads and a great user experience.

Responsive Design: The UI is fully responsive and works seamlessly across desktops, tablets, and mobile devices.

Installation and Setup
Follow these steps to get the frontend running locally.

Prerequisites
Node.js (version 20.x or later recommended)

npm or yarn

Configuration
Clone the repository:

Bash

git clone https://github.com/your-username/your-frontend-repo.git
cd your-frontend-repo
Configure Environment Variables:

Create a file named .env.local in the root of the project.

Add the URL of your running backend API to this file. This tells the Next.js app where to send its API requests.

Code snippet

NEXT_PUBLIC_API_URL=https://localhost:7001
(Replace the URL with your actual backend URL)

Install Dependencies:

Bash

npm install
or if you use yarn:

Bash

yarn install
Run the Development Server:

Bash

npm run dev
or

Bash

yarn dev
Open http://localhost:3000 in your browser to see the application.