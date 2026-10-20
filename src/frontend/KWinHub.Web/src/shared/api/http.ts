import axios, { type AxiosInstance } from 'axios'

const httpClient: AxiosInstance = axios.create({
  baseURL: '/api/v1',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json'
  }
})

httpClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('kwinhub_token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => Promise.reject(error)
)

httpClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('kwinhub_token')
    }
    return Promise.reject(error)
  }
)

export default httpClient