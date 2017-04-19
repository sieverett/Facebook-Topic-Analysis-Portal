import 'whatwg-fetch';
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
        if (value instanceof Date) {
          stringValue = value.toISOString();
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

  var responseTemp;
  return fetch(WebApi + endpoint, {method, headers, body})
    .then(response => {
      // Status Code 200 indicates success.
      if (response.status !== 200) {
        throw new Error(`Invalid status code ${response.status} returned from backend.`);
      }

      responseTemp = response;
      return response.json();
    }).then(json => {
      result(json, null);
    }).catch(function(error) {
      const errorMessage = error.message || 'Fatal Error';
      console.log(error);
      console.log(responseTemp);
      result(null, errorMessage);
    });
};

export function getPost(postId, handler) {
  return sendRequest(`/api/dashboard/post/${postId}`, 'GET', null, handler);
}

export function getPostScrape(scrapeId, handler) {
  return sendRequest(`/api/dashboard/post/scrape/${scrapeId}`, 'GET', null, handler);
}

export function translatePost(postId, handler) {
  return sendRequest(`/api/dashboard/post/translate/${postId}`, 'GET', null, handler);
}

export function getPage(pageId, handler) {
  return sendRequest(`/api/dashboard/page/${pageId}`, 'GET', null, handler);
}

export function getPageScrape(scrapeId, handler) {
  return sendRequest(`/api/dashboard/page/scrape/${scrapeId}`, 'GET', null, handler);
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
        dispatch({ERROR_OCCURED, errorMessage})
      } else {
        dispatch({type, response});
      }
    });
  };
};

export function getPosts(pageNumber, pageSize, since, until) {
  return callAPI('/api/dashboard/post/all', 'GET', {pageNumber, pageSize, since, until}, GET_POSTS_DONE);
}

export function getPostScrapes(pageNumber, pageSize, since, until) {
  return callAPI('/api/dashboard/post/scrape/all', 'GET', {pageNumber, pageSize, since, until}, GET_SCRAPED_POSTS_DONE);
}

export function scrapePosts(since, until) {
  return callAPI('/api/dashboard/post/scrape/scrape', 'POST', {since, until}, SCRAPE_POSTS_DONE);
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

export function getPageScrapes(pageNumber, pageSize, since, until) {
  return callAPI('/api/dashboard/page/scrape/all', 'GET', {pageNumber, pageSize, since, until}, GET_SCRAPED_PAGES_DONE);
}

export function scrapePages(pages) {
  return callAPI('/api/dashboard/page/scrape/scrape', 'POST', pages, SCRAPE_PAGES_DONE);
}
