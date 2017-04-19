import React, { Component } from 'react';
import { scrapePosts } from '../Common/Data/Actions';
import DateRangeForm from '../Components/Common/DateRangeForm';
import Panel from '../Components/Common/Panel';
import PostScrapeHistory from '../Components/PostScrapeHistory';

class ScrapePosts extends Component {
  handleScrapeSubmit = (since, until) => this.context.store.dispatch(scrapePosts(since, until));

  render() {
    return (
      <section>
        <Panel showHeading={false} className="sub-header">
          <DateRangeForm action="Scrape" lowerName="Since" upperName="Until" onSubmit={this.handleScrapeSubmit} />
        </Panel>
        <PostScrapeHistory />
      </section>
    );
  }
}
ScrapePosts.contextTypes = {store: React.PropTypes.object};

export default ScrapePosts;
