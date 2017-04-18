import React, { Component } from 'react';
import Panel from '../Components/Common/Panel';
import ScrapePageForm from '../Components/ScrapePageForm';
import PageScrapeHistory from '../Components/PageScrapeHistory';

class ScrapePages extends Component {
  render() {
    return (
      <section>
        <ScrapePageForm />
        <Panel className="col-md-7" title="Scrape History" table>
        	<PageScrapeHistory />
        </Panel>
      </section>
    );
  }
}

export default ScrapePages;
