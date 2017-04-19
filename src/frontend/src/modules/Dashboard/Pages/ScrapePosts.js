import React, { Component } from 'react';
import { scrapePosts } from '../Common/Data/Actions';
import DateRangeForm from '../Components/Common/DateRangeForm';
import PostScrapeHistory from '../Components/PostScrapeHistory';

class ScrapePosts extends Component {
  handleScrapeSubmit = (since, until) => this.context.store.dispatch(scrapePosts(since, until));

  render() {
    return (
      <section>
        <DateRangeForm action="Scrape" onSubmit={this.handleScrapeSubmit} />
        <PostScrapeHistory />
      </section>
    );
  }
}
ScrapePosts.contextTypes = {store: React.PropTypes.object};

export default ScrapePosts;
