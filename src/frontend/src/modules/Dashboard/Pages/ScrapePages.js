import React, { Component } from 'react';
import { exportPages } from '../Common/Data/Actions'
import Panel from '../Components/Common/Panel';
import DateRangeForm from '../Components/Common/DateRangeForm';
import ScrapePageForm from '../Components/ScrapePageForm';
import PageScrapeHistory from '../Components/PageScrapeHistory';

class ScrapePages extends Component {
  handleExportToCSV = (since, until) => exportPages(since, until, (_, errorMessage) => {});

  render() {
    return (
      <section>
        <ScrapePageForm />
        <section className="col-md-7">
          <Panel title="Export">
            <DateRangeForm action="Export" lowerName="From" upperName="To" allowEmpty={true} onSubmit={this.handleExportToCSV} />
          </Panel>
          <Panel title="Scrape History" table>
        	  <PageScrapeHistory />
          </Panel>
        </section>
      </section>
    );
  }
}

export default ScrapePages;
