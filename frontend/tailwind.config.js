module.exports = {
  content: [
    "./src/**/*.{js,jsx,ts,tsx}",
    "./public/index.html",
  ],
  theme: {
    extend: {
      fontFamily: {
        inter: ['Inter', 'sans-serif'], // Define Inter font
      },
      colors: {
        // Define any custom colors here if needed
        'green-700': '#10B981', // Example: a specific green for branding
        'green-600': '#34D399',
        'blue-600': '#2563EB',
        'purple-600': '#7C3AED',
        'orange-600': '#EA580C',
      },
    },
  },
  plugins: [],
}