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
        Inter: ['Inter', 'ui-sans-serif', 'system-ui'],
        asap: ['Asap'],
        bebas: ['Bebas Neue'],
        gothic: ['League Gothic'],
        marker: ['Permanent Marker'],
        nunito: ['Nunito']
      },
    },
  },
  plugins: [],
};
