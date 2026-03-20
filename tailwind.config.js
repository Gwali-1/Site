module.exports = {
  content: ["./Templates/**/*.cshtml"],
  theme: {
    extend: {
      colors: {
        band: {
          light-silver: "#D3DBDD",
          midnight-green: "#075056",
          gunmetal: "#233038",
          sand-yellow: "#D3DBDD",
        },
      },
      fontFamily: {
        sans: ['Inter', 'ui-sans-serif', 'system-ui', 'sans-serif'],
        heading: ['Montserrat', 'sans-serif'],
        nav: ['Outfit', 'sans-serif'],
        mono: ['"Noto Sans Mono"', 'monospace'],
      },
    },
  },
  plugins: [
    require('@tailwindcss/typography')
  ],
};
