/** @type {import('tailwindcss').Config} */
export default {
  darkMode: ['class', '[data-theme="dark"]'],
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        elevation: {
          1: {
            dark: '#151d28',
            light: '#e3e7eb',
          },
          2: {
            dark: '#1d2837',
            light: '#d4d8db',
          },
          3: {
            dark: '#243247',
            light: '#c5c8cc',
          },
          4: {
            dark: '#2c3d56',
            light: '#b6b9bc',
          },
          5: {
            dark: '#344865',
            light: '#a7aaad',
          },
        },
        brand: {
          light: '#756CB1',
          DEFAULT: '#795EA9',
          dark: '#795EA9',
          'extra-light': '#A69BF5',
        },
        info: {
          dark: '#008BD7',
          light: '#009CE7',
        },
        success: {
          dark: '#29903B',
          light: '#1C8139',
        },
        warning: {
          dark: '#E3B341',
          light: '#EAC54F',
        },
        danger: {
          dark: '#B62324',
          light: '#A40E26',
        },
        fg: {
          dark: '#EFEFF6',
          light: '#27313f',
        },
        bg: {
          dark: '#151d28',
          light: '#e3e7eb',
        },
        border: {
          dark: '#243247',
          light: '#c5c8cc',
        },
      },
      backgroundImage: {
        'brand-gradient': 'linear-gradient(135deg, #756CB1 0%, #795EA9 100%)',
        'border-gradient': 'linear-gradient(135deg, #756CB1 0%, #795EA9 100%)',
      },
      borderRadius: {
        'sm': '4px',
        'DEFAULT': '6px',
        'md': '8px',
        'lg': '12px',
        'full': '50%',
      },
      boxShadow: {
        'brand': '0 2px 8px rgba(121, 94, 169, 0.2)',
        'brand-lg': '0 4px 12px rgba(121, 94, 169, 0.3)',
      },
    },
  },
  plugins: [],
}
