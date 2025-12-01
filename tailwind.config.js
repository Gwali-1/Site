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
        noto: ['Noto Sans Mono', 'sans-serif'],
        asap: ['Asap'],
        gothic: ['League Gothic'],
        marker: ['"Permanent Marker"', 'cursive'],
        inter: ['Inter'],
        robot: ['Roboto'],
        nunito: ['Nunito']
      },
    },
  },
  plugins: [
    require('@tailwindcss/typography')
  ],
};
