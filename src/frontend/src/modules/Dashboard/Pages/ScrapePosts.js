import React, { Component } from 'react';
import { scrapePosts } from '../Common/Data/Actions';
import DateRangeForm from '../Components/Common/DateRangeForm';
import PostScrapeHistory from '../Components/PostScrapeHistory';

class ScrapePosts extends Component {
  handleScrapeSubmit = (since, until) => scrapePosts(since, until);

  render() {
    return (
      <section>
        <DateRangeForm action="Scrape" onSubmit={this.handleScrapeSubmit} />
        <PostScrapeHistory />
      </section>
    );
  }
}

export default ScrapePosts;
