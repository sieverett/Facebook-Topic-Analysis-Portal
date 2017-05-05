import 'whatwg-fetch';
import moment from 'moment';
import { WebApi } from '../../../../config';
import * as es from './ElasticSearch';

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
        if (value === undefined) {
          continue;
        }

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

  const download = (response, name) => {
    return response.blob().then(blob => {
      const url = window.URL.createObjectURL(blob);
      
      let a = document.createElement("a");
      document.body.appendChild(a);
      a.style = 'display: none';
      a.href = url;
      a.download = name;
      a.click();

      window.URL.revokeObjectURL(url);
    });
  };

  let responseTemp;
  return fetch(WebApi + endpoint, {method, headers, body})
    .then(response => {
      responseTemp = response;

      // Status Code 200 indicates success.
      if (response.status !== 200) {
        throw new Error(`Invalid status code ${response.status} returned from the backend.`);
      }

      const contentType = response.headers.get('content-type');
      if (contentType.includes('text/csv')) {
        download(response, 'export.csv');
      } else if (contentType.includes('application/json-download')) {
        download(response, 'export.json');
      } else if (contentType.includes('application/json')) {
        return response.json().then(json => {
          result(json, null);
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
export const GET_COMMENTS_DONE = 'GET_COMMENTS_DONE';
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

export function createQuery(key, since, until, extra) {
  const extraOrEmpty = extra || [];
  return es.bool([es.dateRange(key, since, until), ...extraOrEmpty]);
}

export function createSort(orderingKey, descending) {
  return es.sort([{key: orderingKey, descending}]);
}

// Section: scraping posts.
export function getPost(postId, handler) {
  return sendRequest(`/api/dashboard/scrape/post/${postId}`, 'GET', null, handler);
}

export function getPosts(pageNumber, pageSize, since, until, pages, orderingKey, descending) {
  const extra = pages ? [es.terms('page.facebookId', pages)] : [];
  const query = createQuery('created_time', since, until, extra);
  const sort = createSort(orderingKey || 'created_time', descending);
  return callAPI('/api/dashboard/scrape/post/all', 'POST', {pageNumber, pageSize, query, sort}, GET_POSTS_DONE);
}

export function getPostComments(postId, pageNumber, pageSize) {
  return callAPI(`/api/dashboard/scrape/comment/post/${postId}`, 'POST', {postId, pageNumber, pageSize}, GET_COMMENTS_DONE);
}

export function scrapePosts(since, until) {
  return callAPI('/api/dashboard/scrape/post/scrape', 'POST', {since, until}, SCRAPE_POSTS_DONE);
}

export function exportPosts(contentType, since, until, pages, handler) {
  const extra = pages ? [es.terms('page.facebookId', pages)] : [];
  const query = createQuery('created_time', since, until, extra);
  const sort = createSort('created_time', true);
  return sendRequest(`/api/dashboard/scrape/post/export/${contentType}`, 'POST', {query, sort}, null, handler);
}

// Section: post scraping history.
export function getPostScrapes(pageNumber, pageSize, since, until) {
  const query = createQuery('importStart', since, until);
  return callAPI('/api/dashboard/scrape/post/history/all', 'POST', {pageNumber, pageSize, query}, GET_SCRAPED_POSTS_DONE);
}

export function getPostScrape(scrapeId, handler) {
  return sendRequest(`/api/dashboard/scrape/post/history/${scrapeId}`, 'GET', null, handler);
}

// Section: scraping comments.
export function getComments(pageNumber, pageSize, since, until, orderingKey, descending) {
  const query = createQuery('created_time', since, until);
  const sort = createSort(orderingKey || 'created_time', descending);
  return callAPI('/api/dashboard/scrape/comment/all', 'POST', {pageNumber, pageSize, query, sort}, GET_COMMENTS_DONE);
}

export function exportComments(contentType, since, until, handler) {
  return sendRequest(`/api/dashboard/scrape/comment/export/${contentType}`, 'POST', {since, until}, null, handler);
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
  const query = createQuery('created', since, until);
  return callAPI('/api/dashboard/page/all', 'POST', {pageNumber, pageSize, query}, GET_PAGES_DONE);
}

// Section: scraping pages.
export function scrapePages(pages) {
  return callAPI('/api/dashboard/scrape/page/scrape', 'POST', pages, SCRAPE_PAGES_DONE);
}

export function exportPages(contentType, since, until, handler) {
  return sendRequest(`/api/dashboard/scrape/page/history/export/${contentType}`, 'POST', {since, until}, null, handler);
}

// Section: page scraping history.
export function getPageScrapes(pageNumber, pageSize, since, until) {
  const query = createQuery('importStart', since, until);
  return callAPI('/api/dashboard/scrape/page/history/all', 'POST', {pageNumber, pageSize, query}, GET_SCRAPED_PAGES_DONE);
}

export function getPageScrape(scrapeId, handler) {
  return sendRequest(`/api/dashboard/scrape/page/history/${scrapeId}`, 'GET', null, handler);
}
