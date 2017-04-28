let WebApi;
if (!process.env.NODE_ENV || process.env.NODE_ENV === 'development') {
    WebApi = 'http://localhost:50547';
} else {
    WebApi = 'http://ec2-54-187-166-5.us-west-2.compute.amazonaws.com:50547';
}

export { WebApi };
