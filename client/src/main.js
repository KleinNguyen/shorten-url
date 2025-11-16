import { createApp } from 'vue'
import App from './App.vue'

import router from './router'  

import 'semantic-ui-css/semantic.min.css';

import 'sweetalert2/dist/sweetalert2.min.css';
const Swal = require('sweetalert2')

const app = createApp(App)
app.use(router)
app.mount('#app')



