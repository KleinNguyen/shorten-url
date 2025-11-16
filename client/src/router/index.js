import { createRouter, createWebHistory } from 'vue-router';

import HomeView from '../Views/HomeView.vue';
import SignInView from '../Views/Auth/SignInView.vue';
import SignUpView from '../Views/Auth/SignUpView.vue';
import ForgotPasswordView from '../Views/Auth/ForgotPasswordView.vue';
import PersonalView from '../Views/Auth/PersonalView.vue';
import DashboardView from '../Views/DashboardView.vue';
import ResetPasswordView from '../Views/Auth/ResetPasswordView.vue';

const routes = [
    {
        path: '/',
        redirect: '/home',
    },
    {
        path: '/home',
        name: 'Home',
        component: HomeView
    },
    {
        path: '/signin',
        name: 'SignIn',
        component: SignInView
    },
    {
        path: '/signup',
        name: 'SignUp',
        component: SignUpView
    },
    {
        path: '/forgot-password',
        name: 'ForgotPassword',
        component: ForgotPasswordView
    },
    {
        path: '/personal',
        name: 'Personal',
        component: PersonalView
    },
    {
        path: '/dashboard',
        name: 'Dashboard',
        component: DashboardView
    },
    {
        path: '/reset-password',
        name: 'ResetPassword',
        component: ResetPasswordView
    },
];

const router = createRouter({
    history: createWebHistory(),
    routes,
});

export default router;
