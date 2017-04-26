import 'whatwg-fetch';
import moment from 'moment';
import { WebApi } from '../../../../config';

export const sendRequest = (endpoint, method, params, result) => {
  var headers;
  var body;

  // Construct a request from the params. This depends on the HTTP method.
  if (params) {
    if (method === 'GET') {
      // GET requires a list of `?name1=value1&name2=value`.
      var query = '?';
      for (const key in params) {
        const value = params[key];
        let stringValue;

        const dateValue = moment(value, moment.ISO_8601);
        if (value instanceof Date) {
          stringValue = value.toISOString();
        } else if (dateValue.isValid()) {
          stringValue = dateValue.toISOString();
        } else {
          stringValue = value;
        }

        query += `${key}=${stringValue}&`;
      }

      endpoint += query;
    } else if (method === 'POST' || method === 'PATCH') {
      // POST and PATCH require a JSON encoded body.
      body = JSON.stringify(params);
      headers = {'Content-Type': 'application/json' };
    }
  }

  let responseTemp;
  return fetch(WebApi + endpoint, {method, headers, body})
    .then(response => {
      responseTemp = response;

      // Status Code 200 indicates success.
      if (response.status !== 200) {
        throw new Error(`Invalid status code ${response.status} returned from the backend.`);
      }

      const contentType = response.headers.get('content-type');
      if (contentType.includes('application/json')) {
        return response.json().then(json => {
          result(json, null);
        });
      } else if (contentType.includes('text/csv')) {
        return response.blob().then(blob => {
          const url = window.URL.createObjectURL(blob);
          
          let a = document.createElement("a");
          document.body.appendChild(a);
          a.style = 'display: none';
          a.href = url;
          a.download = 'export.csv';
          a.click();

          window.URL.revokeObjectURL(url);
        });
      } else {
        throw new Error(`Invalid content type ${contentType} returned from the backend.`);
      }
    }).catch(function(error) {
      const errorMessage = error.message || 'Fatal Error';
      console.log(error);
      console.log(responseTemp);
      result(null, errorMessage);
    });
};

export function translate(message, handler) {
  return sendRequest(`/api/dashboard/translate/${message}`, 'GET', null, handler);
}

export const ERROR_OCCURED = 'ERROR_OCCURED';

export const GET_POSTS_DONE = 'GET_POSTS_DONE';
export const SCRAPE_POSTS_DONE = 'SCRAPE_POSTS_DONE';
export const GET_SCRAPED_POSTS_DONE = 'GET_SCRAPED_POSTS_DONE';

export const NEW_PAGE_DONE = 'NEW_PAGE_DONE';
export const NEW_PAGES_DONE = 'NEW_PAGES_DONE';
export const EDIT_PAGE_DONE = 'EDIT_PAGE_DONE';
export const GET_PAGES_DONE = 'GET_PAGES_DONE';
export const DELETE_PAGE_DONE = 'DELETE_PAGE_DONE';

export const SCRAPE_PAGES_DONE = 'SCRAPE_PAGES_DONE';
export const GET_SCRAPED_PAGES_DONE = 'GET_SCRAPED_PAGES_DONE';

const callAPI = (endpoint, method, params, type) => {
  return dispatch => {
    return sendRequest(endpoint, method, params, (response, errorMessage) => {
      if (errorMessage) {
        dispatch({type: ERROR_OCCURED, errorMessage})
      } else {
        dispatch({type, response});
      }
    });
  };
};

// Section: scraping posts.
export function getPost(postId, handler) {
  return sendRequest(`/api/dashboard/scrape/post/${postId}`, 'GET', null, handler);
}

export function getPosts(pageNumber, pageSize, since, until) {
  return callAPI('/api/dashboard/scrape/post/all', 'GET', {pageNumber, pageSize, since, until}, GET_POSTS_DONE);
}

export function scrapePosts(since, until) {
  return callAPI('/api/dashboard/scrape/post/scrape', 'POST', {since, until}, SCRAPE_POSTS_DONE);
}

export function exportPosts(since, until, handler) {
  return sendRequest('/api/dashboard/scrape/post/export', 'GET', {since, until}, null, handler);
}

// Section: post scraping history.
export function getPostScrapes(pageNumber, pageSize, since, until) {
  return callAPI('/api/dashboard/scrape/post/history/all', 'GET', {pageNumber, pageSize, since, until}, GET_SCRAPED_POSTS_DONE);
}

export function getPostScrape(scrapeId, handler) {
  return sendRequest(`/api/dashboard/scrape/post/history/${scrapeId}`, 'GET', null, handler);
}

// Section: pages to scrape.
export function getPage(pageId, handler) {
  return sendRequest(`/api/dashboard/page/${pageId}`, 'GET', null, handler);
}

export function newPage(name, facebookId) {
  return callAPI('/api/dashboard/page/new', 'POST', {name, facebookId}, NEW_PAGE_DONE);
}

export function newPages(pages) {
  return callAPI('/api/dashboard/page/new/multiple', 'POST', pages, NEW_PAGES_DONE);
}

export function editPage(id, name, facebookId) {
  return callAPI(`/api/dashboard/page/${id}`, 'PATCH', {name, facebookId}, EDIT_PAGE_DONE);
}

export function deletePage(id) {
  return callAPI(`/api/dashboard/page/${id}`, 'DELETE', {name, id}, DELETE_PAGE_DONE);
}

export function getPages(pageNumber, pageSize, since, until) {
  return callAPI('/api/dashboard/page/all', 'GET', {pageNumber, pageSize, since, until}, GET_PAGES_DONE);
}

// Section: scraping pages.
export function scrapePages(pages) {
  return callAPI('/api/dashboard/scrape/page/scrape', 'POST', pages, SCRAPE_PAGES_DONE);
}

export function exportPages(since, until, handler) {
  return sendRequest('/api/dashboard/scrape/page/history/export', 'GET', {since, until}, null, handler);
}

// Section: page scraping history.
export function getPageScrapes(pageNumber, pageSize, since, until) {
  return callAPI('/api/dashboard/scrape/page/history/all', 'GET', {pageNumber, pageSize, since, until}, GET_SCRAPED_PAGES_DONE);
}

export function getPageScrape(scrapeId, handler) {
  return sendRequest(`/api/dashboard/scrape/page/history/${scrapeId}`, 'GET', null, handler);
}
